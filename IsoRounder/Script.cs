using System;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

[assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS
{
    public class Script
    {
        public Script() { } // constructor

        // variables
        private Patient _patient;
        private PlanSetup _plan;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Execute(ScriptContext context)
        {
            try
            {
                ValidatePatient(context);
                ValidatePlan(context);

                // retrieve patient, plan, and field info
                _patient = GetPatient(context);
                _plan = GetPlanSetup(context);

                // get current isocenter
                VVector oldIso = _plan.Beams.First().GetEditableParameters().Isocenter; // in mm

                // round to nearest .5cm aka 5mm
                double newIsoX = Math.Round(oldIso[0] / 5.0) * 5.0;
                double newIsoY = Math.Round(oldIso[1] / 5.0) * 5.0;
                double newIsoZ = Math.Round(oldIso[2] / 5.0) * 5.0;

                VVector newIso = new VVector(newIsoX, newIsoY, newIsoZ); // in mm
                
                _patient.BeginModifications();

                // only need to change first beam; changing one iso changes all the others
                BeamParameters beamParams = _plan.Beams.First().GetEditableParameters();
                beamParams.Isocenter = newIso;
                _plan.Beams.First().ApplyParameters(beamParams);

                MessageBox.Show($"Plan isocenter successfully changed from ({oldIso[0] / 10}, {oldIso[1] / 10}, {oldIso[2] / 10}) " +
                                $"to ({newIso[0] / 10}, {newIso[1] / 10}, {newIso[2] / 10}).");

            } // try
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while executing the script.", ex);
            } // catch

        } // Execute()

        #region helper methods

        #region validators
        private void ValidatePlan(ScriptContext context)
        {
            if (context.PlanSetup == null)
            {
                throw new ApplicationException("No plan is currently open. Please open a plan and try again.");
            }
        } // ValidatePlan()

        private void ValidatePatient(ScriptContext context)
        {
            if (context.Patient == null)
            {
                throw new ApplicationException("No patient is currently open. Please open a patient and try again.");
            }
        } // ValidatePatient()

        #endregion validators

        #region getters
        private static Patient GetPatient(ScriptContext context)
        {
            return context.Patient;
        } // GetPatient()

        private static PlanSetup GetPlanSetup(ScriptContext context)
        {
            return context.PlanSetup;
        } // GetPlanSetup()

        #endregion getters

        #endregion helper methods

    } // class Script
} // namespace VMS.TPS