 //Interneuron synapse

//Copyright(C) 2024 Interneuron Limited

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

//See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
﻿using Interneuron.Common.Extensions;
using Microsoft.CodeAnalysis.FlowAnalysis;
using SynapseStudioWeb.AppCode.Constants;
using SynapseStudioWeb.DataService.APIModel;
using SynapseStudioWeb.Helpers;
using SynapseStudioWeb.Models.MedicationMgmt;
using SynapseStudioWeb.Models.MedicinalMgmt;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SynapseStudioWeb.Controllers
{
    internal class BulkEditVMBuilder
    {
        public BulkEditVMBuilder()
        {
        }

        internal void Build(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm)
        {
            BuildLocalLicensedUse(formularies, vm);
            BuildLocalUnLicensedUse(formularies, vm);

            BuildLocalRoute(formularies, vm, TerminologyConstants.ROUTEFIELDTYPE_NORMAL_CD, (vmInput) => vmInput.DisplayLocalLicensedRouteLbl = true, (vmInput, routes) =>
            {
                vmInput.LocalLicensedRoute = new List<CodeNameSelectorModel>();
                routes?.Each(rec =>
                {
                    var cns = new CodeNameSelectorModel() { Id = rec.RouteCd, Name = rec.RouteDesc, Source = rec.Source };
                    vmInput.LocalLicensedRoute.Add(cns);
                });
            });
            BuildLocalRoute(formularies, vm, TerminologyConstants.ROUTEFIELDTYPE_UNLICENSED_CD, (vmInput) => vmInput.DisplayLocalUnlicensedRouteLbl = true, (vmInput, routes) =>
            {
                vmInput.LocalUnlicensedRoute = new List<CodeNameSelectorModel>();
                routes?.Each(rec =>
                {
                    var cns = new CodeNameSelectorModel() { Id = rec.RouteCd, Name = rec.RouteDesc, Source = rec.Source };
                    vmInput.LocalUnlicensedRoute.Add(cns);
                });
            });

            BuildCustomWarnings(formularies, vm);
            BuildReminders(formularies, vm);
            BuildEndorsements(formularies, vm);

            BuildMedusaPreparationInstructions(formularies, vm);
            BuildTitrationTypes(formularies, vm);
            BuildRoundingFactorCd(formularies, vm);
            BuildDiluents(formularies, vm);
            BuildClinicalTrialMedication(formularies, vm);
            
            BuildGastroResistant(formularies, vm);
            BuildCriticalDrug(formularies, vm);
            BuildModifiedRelease(formularies, vm);
            BuildExpensiveMedication(formularies, vm);
            BuildHighAlertMedication(formularies, vm);
            
            BuildIvToOral(formularies, vm);
            BuildNotForPrn(formularies, vm);
            BuildIsBloodProduct(formularies, vm);

            BuildIsDiluent(formularies, vm);
            BuildPrescribable(formularies, vm);
            BuildOutpatientMedicationCd(formularies, vm);
            BuildIgnoreDuplicateWarnings(formularies, vm);

            BuildIsCustomControlledDrug(formularies, vm);
            BuildIsPrescriptionPrintingRequired(formularies, vm);
            BuildIsIndicationMandatory(formularies, vm);

            BuildWitnessingRequired(formularies, vm);
            BuildRnohFormularyStatuscd(formularies, vm);
        }

        private void BuildLocalLicensedUse(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm)
        {
            var firstLocalLicUse = formularies[0].Detail.LocalLicensedUses?.Select(x => x.Cd).Distinct().ToHashSet();

            if (formularies.Count > 1)
            {
                for (int formularyCnt = 1; formularyCnt < formularies.Count; formularyCnt++)
                {
                    var lc = formularies[formularyCnt].Detail.LocalLicensedUses?.Select(x => x.Cd).Distinct().ToList();

                    var hasMismatch = (!firstLocalLicUse.IsCollectionValid() && lc.IsCollectionValid()) || (firstLocalLicUse.IsCollectionValid() && !lc.IsCollectionValid()) || ((lc.IsCollectionValid() && firstLocalLicUse.IsCollectionValid()) && (lc.Count != firstLocalLicUse.Count)) || ((lc.IsCollectionValid() && firstLocalLicUse.IsCollectionValid()) && (lc.Any(rec => !firstLocalLicUse.Contains(rec))));
                    if (hasMismatch)
                    {
                        vm.DisplayLocalLicensedIndicationLbl = true;
                        return;
                    }
                }
            }

            vm.LocalLicensedUse = new List<CodeNameSelectorModel>();
            formularies[0].Detail.LocalLicensedUses?.Each(rec =>
            {
                var cns = new CodeNameSelectorModel() { Id = rec.Cd, Name = rec.Desc, Source = rec.Source };
                vm.LocalLicensedUse.Add(cns);
            });
        }

        private void BuildLocalUnLicensedUse(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm)
        {
            var first = formularies[0].Detail.LocalUnLicensedUses?.Select(x => x.Cd).Distinct().ToHashSet();

            if (formularies.Count > 1)
            {
                for (int formularyCnt = 1; formularyCnt < formularies.Count; formularyCnt++)
                {
                    var others = formularies[formularyCnt].Detail.LocalUnLicensedUses?.Select(x => x.Cd).Distinct().ToList();

                    var hasMismatch = (!first.IsCollectionValid() && others.IsCollectionValid()) || (first.IsCollectionValid() && !others.IsCollectionValid()) || ((others.IsCollectionValid() && first.IsCollectionValid()) && (others.Count != first.Count)) || ((others.IsCollectionValid() && first.IsCollectionValid()) && (others.Any(rec => !first.Contains(rec))));

                    if (hasMismatch)
                    {
                        vm.DisplayLocalUnlicensedIndicationLbl = true;
                        return;
                    }
                }
            }

            vm.LocalUnlicensedUse = new List<CodeNameSelectorModel>();
            formularies[0].Detail.LocalUnLicensedUses?.Each(rec =>
            {
                var cns = new CodeNameSelectorModel() { Id = rec.Cd, Name = rec.Desc, Source = rec.Source };
                vm.LocalUnlicensedUse.Add(cns);
            });
        }

        private void BuildLocalRoute(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm, string routeFieldTypeCd, Action<BulkFormularyEditModel> onMismatch, Action<BulkFormularyEditModel, List<FormularyLocalRouteDetailAPIModel>> onMatch)
        {
            var firstRoute = formularies[0].FormularyLocalRouteDetails?.Where(rec => rec.RouteFieldTypeCd == routeFieldTypeCd).ToList();
            var first = firstRoute?.Select(x => x.RouteCd).Distinct().ToHashSet();

            if (formularies.Count > 1)
            {
                for (int formularyCnt = 1; formularyCnt < formularies.Count; formularyCnt++)
                {
                    var lc = formularies[formularyCnt].FormularyLocalRouteDetails?.Where(rec => rec.RouteFieldTypeCd == routeFieldTypeCd)?.Select(x => x.RouteCd).Distinct().ToList();


                    var hasMisMatch = (!first.IsCollectionValid() && lc.IsCollectionValid()) || (first.IsCollectionValid() && !lc.IsCollectionValid()) || ((lc.IsCollectionValid() && first.IsCollectionValid()) && (lc.Count != first.Count)) || ((lc.IsCollectionValid() && first.IsCollectionValid()) && (lc.Any(rec => !first.Contains(rec))));

                    if (hasMisMatch)
                    {
                        onMismatch(vm);
                        return;
                    }
                }
            }
            onMatch(vm, firstRoute);
        }

        private void BuildCustomWarnings(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm)
        {
            var firstCw = formularies[0].Detail.CustomWarnings;

            var first = firstCw?.Select(x => x.Warning).Distinct().ToHashSet();

            if (formularies.Count > 1)
            {
                for (int formularyCnt = 1; formularyCnt < formularies.Count; formularyCnt++)
                {
                    var others = formularies[formularyCnt].Detail.CustomWarnings?.Select(x => x.Warning).Distinct().ToList();

                    var hasMismatch = (!first.IsCollectionValid() && others.IsCollectionValid()) || (first.IsCollectionValid() && !others.IsCollectionValid()) || ((others.IsCollectionValid() && first.IsCollectionValid()) && (others.Count != first.Count)) || ((others.IsCollectionValid() && first.IsCollectionValid()) &&  (others.Any(rec => !first.Contains(rec))));

                    if (hasMismatch)
                    {
                        vm.DisplayCustomWarningsLbl = true;
                        return;
                    }
                }
            }

            vm.CustomWarnings = new List<FormularyCustomWarningModel>();
            firstCw?.Each(rec =>
            {
                var cns = new FormularyCustomWarningModel() { Warning = rec.Warning, NeedResponse = rec.NeedResponse == true, Source = rec.Source };
                vm.CustomWarnings.Add(cns);
            });
        }

        private void BuildReminders(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm)
        {
            var firstCw = formularies[0].Detail.Reminders;

            var first = firstCw?.Select(x => x.Reminder).Distinct().ToHashSet();

            if (formularies.Count > 1)
            {
                for (int formularyCnt = 1; formularyCnt < formularies.Count; formularyCnt++)
                {
                    var others = formularies[formularyCnt].Detail.Reminders?.Select(x => x.Reminder).Distinct().ToList();

                    var hasMismatch = (!first.IsCollectionValid() && others.IsCollectionValid()) || (first.IsCollectionValid() && !others.IsCollectionValid()) || ((others.IsCollectionValid() && first.IsCollectionValid()) && (others.Count != first.Count)) || ((others.IsCollectionValid() && first.IsCollectionValid()) && (others.Any(rec => !first.Contains(rec))));

                    if (hasMismatch)
                    {
                        vm.DisplayRemindersLbl = true;
                        return;
                    }
                }
            }

            vm.Reminders = new List<FormularyReminderModel>();
            firstCw?.Each(rec =>
            {
                var cns = new FormularyReminderModel() { Reminder = rec.Reminder, Duration = rec.Duration, Active = rec.Active == true, Source = rec.Source };
                vm.Reminders.Add(cns);
            });
        }

        private void BuildEndorsements(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm)
        {
            var firstCw = formularies[0].Detail.Endorsements;

            var first = firstCw?.Select(x => x).Distinct().ToHashSet();

            if (formularies.Count > 1)
            {
                for (int formularyCnt = 1; formularyCnt < formularies.Count; formularyCnt++)
                {
                    var others = formularies[formularyCnt].Detail.Endorsements?.Select(x => x).Distinct().ToList();

                    var hasMismatch = (!first.IsCollectionValid() && others.IsCollectionValid()) || (first.IsCollectionValid() && !others.IsCollectionValid()) || ((others.IsCollectionValid() && first.IsCollectionValid()) && (others.Count != first.Count)) || ((others.IsCollectionValid() && first.IsCollectionValid()) &&  (others.Any(rec => !first.Contains(rec))));

                    if (hasMismatch)
                    {
                        vm.DisplayEndorsementsLbl = true;
                        return;
                    }
                }
            }

            vm.Endorsements = new List<string>();
            firstCw?.Each(rec => vm.Endorsements.Add(rec));
        }

        private void BuildMedusaPreparationInstructions(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm)
        {
            var firstCw = formularies[0].Detail.MedusaPreparationInstructions;

            var first = firstCw?.Select(x => x).Distinct().ToHashSet();

            if (formularies.Count > 1)
            {

                for (int formularyCnt = 1; formularyCnt < formularies.Count; formularyCnt++)
                {
                    var others = formularies[formularyCnt].Detail.MedusaPreparationInstructions?.Select(x => x).Distinct().ToList();

                    var hasMismatch = (!first.IsCollectionValid() && others.IsCollectionValid()) || (first.IsCollectionValid() && !others.IsCollectionValid()) || ((others.IsCollectionValid() && first.IsCollectionValid()) && (others.Count != first.Count)) || ((others.IsCollectionValid() && first.IsCollectionValid()) && (others.Any(rec => !first.Contains(rec))));

                    if (hasMismatch)
                    {
                        vm.DisplayMedusaPreparationLbl = true;
                        return;
                    }
                }
            }

            vm.MedusaPreparationInstructionsEditable = firstCw?.FirstOrDefault();
        }

        private void BuildTitrationTypes(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm)
        {
            var firstCw = formularies[0].Detail.TitrationTypes;

            var first = firstCw?.Select(x => x.Cd).Distinct().ToHashSet();

            if (formularies.Count > 1)
            {
                for (int formularyCnt = 1; formularyCnt < formularies.Count; formularyCnt++)
                {
                    var others = formularies[formularyCnt].Detail.TitrationTypes?.Select(x => x.Cd).Distinct().ToList();

                    var hasMismatch = (!first.IsCollectionValid() && others.IsCollectionValid()) || (first.IsCollectionValid() && !others.IsCollectionValid()) || ((others.IsCollectionValid() && first.IsCollectionValid()) && (others.Count != first.Count)) || ((others.IsCollectionValid() && first.IsCollectionValid()) &&  (others.Any(rec => !first.Contains(rec))));

                    if (hasMismatch)
                    {
                        vm.DisplayTitrationTypeLbl = true;
                        return;
                    }
                }
            }

            vm.TitrationTypesEditableId = firstCw?.FirstOrDefault()?.Cd;
        }

        private void BuildRoundingFactorCd(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm)
        {
            var first = formularies[0].Detail.RoundingFactorCd;

            if (formularies.Count > 1)
            {
                for (int formularyCnt = 1; formularyCnt < formularies.Count; formularyCnt++)
                {
                    var others = formularies[formularyCnt].Detail.RoundingFactorCd;

                    var hasMismatch = (!first.IsEmpty() && others.IsEmpty()) || (first.IsEmpty() && !others.IsEmpty()) || (first != others);

                    if (hasMismatch)
                    {
                        vm.DisplayRoundingFactorLbl = true;
                        return;
                    }
                }
            }

            vm.RoundingFactorCd = first;
        }

        private void BuildDiluents(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm)
        {
            var first = formularies[0].Detail.Diluents?.Select(x => x.Cd).Distinct().ToHashSet();

            if (formularies.Count > 1)
            {
                for (int formularyCnt = 1; formularyCnt < formularies.Count; formularyCnt++)
                {
                    var others = formularies[formularyCnt].Detail.Diluents?.Select(x => x.Cd).Distinct().ToList();

                    var hasMismatch = (!first.IsCollectionValid() && others.IsCollectionValid()) || (first.IsCollectionValid() && !others.IsCollectionValid()) || ((others.IsCollectionValid() && first.IsCollectionValid()) && (others.Count != first.Count)) || ((others.IsCollectionValid() && first.IsCollectionValid()) && (others.Any(rec => !first.Contains(rec))));

                    if (hasMismatch)
                    {
                        vm.DisplayCompatibleDiluentLbl = true;
                        return;
                    }
                }
            }

            vm.Diluents = new List<CodeNameSelectorModel>();
            formularies[0].Detail.Diluents?.Each(rec =>
            {
                var cns = new CodeNameSelectorModel() { Id = rec.Cd, Name = rec.Desc, Source = rec.Source };
                vm.Diluents.Add(cns);
            });
        }

        private void BuildClinicalTrialMedication(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm)
        {
            var first = formularies[0].Detail.ClinicalTrialMedication;

            if (formularies.Count > 1)
            {
                for (int formularyCnt = 1; formularyCnt < formularies.Count; formularyCnt++)
                {
                    var others = formularies[formularyCnt].Detail.ClinicalTrialMedication;

                    var hasMismatch = (!first.IsEmpty() && others.IsEmpty()) || (first.IsEmpty() && !others.IsEmpty()) || (others != first);

                    if (hasMismatch)
                    {
                        vm.DisplayClinicalTrialMedicationLbl = true;
                        return;
                    }
                }
            }

            vm.NullableClinicalTrialMedication = first.ToNullable<bool>();
        }

        private void BuildGastroResistant(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm)
        {
            var first = formularies[0].Detail.IsGastroResistant;

            if (formularies.Count > 1)
            {
                for (int formularyCnt = 1; formularyCnt < formularies.Count; formularyCnt++)
                {
                    var others = formularies[formularyCnt].Detail.IsGastroResistant;

                    var hasMismatch = (!first.HasValue && others.HasValue) || (first.HasValue && !others.HasValue) || (others != first);

                    if (hasMismatch)
                    {
                        vm.DisplayGastroResistantLbl = true;
                        return;
                    }
                }
            }

            vm.NullableIsGastroResistant = first;
        }

        private void BuildCriticalDrug(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm)
        {
            var first = formularies[0].Detail.CriticalDrug;

            if (formularies.Count > 1)
            {
                for (int formularyCnt = 1; formularyCnt < formularies.Count; formularyCnt++)
                {
                    var others = formularies[formularyCnt].Detail.CriticalDrug;

                    var hasMismatch = (!first.IsEmpty() && others.IsEmpty()) || (first.IsEmpty() && !others.IsEmpty()) || (others != first);

                    if (hasMismatch)
                    {
                        vm.DisplayCriticalDrugLbl = true;
                        return;
                    }
                }
            }

            vm.NullableCriticalDrug = first.ToNullable<bool>();
        }

        private void BuildModifiedRelease(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm)
        {
            var first = formularies[0].Detail.IsModifiedRelease;

            if (formularies.Count > 1)
            {
                for (int formularyCnt = 1; formularyCnt < formularies.Count; formularyCnt++)
                {
                    var others = formularies[formularyCnt].Detail.IsModifiedRelease;

                    var hasMismatch = (!first.HasValue && others.HasValue) || (first.HasValue && !others.HasValue) || (others != first);

                    if (hasMismatch)
                    {
                        vm.DisplayModifiedReleaseLbl = true;
                        return;
                    }
                }
            }

            vm.NullableIsModifiedRelease = first;
        }

        private void BuildExpensiveMedication(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm)
        {
            var first = formularies[0].Detail.ExpensiveMedication;

            if (formularies.Count > 1)
            {
                for (int formularyCnt = 1; formularyCnt < formularies.Count; formularyCnt++)
                {
                    var others = formularies[formularyCnt].Detail.ExpensiveMedication;

                    var hasMismatch = (!first.IsEmpty() && others.IsEmpty()) || (first.IsEmpty() && !others.IsEmpty()) || (others != first);

                    if (hasMismatch)
                    {
                        vm.DisplayExpensiveMedicationLbl = true;
                        return;
                    }
                }
            }

            vm.NullableExpensiveMedication = first.ToNullable<bool>();
        }

        private void BuildHighAlertMedication(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm)
        {
            var first = formularies[0].Detail.HighAlertMedication;

            if (formularies.Count > 1)
            {
                for (int formularyCnt = 1; formularyCnt < formularies.Count; formularyCnt++)
                {
                    var others = formularies[formularyCnt].Detail.HighAlertMedication;

                    var hasMismatch = (!first.IsEmpty() && others.IsEmpty()) || (first.IsEmpty() && !others.IsEmpty()) || (others != first);

                    if (hasMismatch)
                    {
                        vm.DisplayHighAlertMedicationLbl = true;
                        return;
                    }
                }
            }

            vm.NullableHighAlertMedication = first.ToNullable<bool>();
        }

        private void BuildIvToOral(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm)
        {
            var first = formularies[0].Detail.IvToOral;

            if (formularies.Count > 1)
            {
                for (int formularyCnt = 1; formularyCnt < formularies.Count; formularyCnt++)
                {
                    var others = formularies[formularyCnt].Detail.IvToOral;

                    var hasMismatch = (!first.IsEmpty() && others.IsEmpty()) || (first.IsEmpty() && !others.IsEmpty()) || (others != first);

                    if (hasMismatch)
                    {
                        vm.DisplayIVToOralLbl = true;
                        return;
                    }
                }
            }

            vm.NullableIVToOral = first.ToNullable<bool>();
        }

        private void BuildNotForPrn(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm)
        {
            var first = formularies[0].Detail.NotForPrn;

            if (formularies.Count > 1)
            {
                for (int formularyCnt = 1; formularyCnt < formularies.Count; formularyCnt++)
                {
                    var others = formularies[formularyCnt].Detail.NotForPrn;

                    var hasMismatch = (!first.IsEmpty() && others.IsEmpty()) || (first.IsEmpty() && !others.IsEmpty()) || (others != first);

                    if (hasMismatch)
                    {
                        vm.DisplayNotForPRNLbl = true;
                        return;
                    }
                }
            }

            vm.NullableNotForPrn = first.ToNullable<bool>();
        }

        private void BuildIsBloodProduct(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm)
        {
            var first = formularies[0].Detail.IsBloodProduct;

            if (formularies.Count > 1)
            {
                for (int formularyCnt = 1; formularyCnt < formularies.Count; formularyCnt++)
                {
                    var others = formularies[formularyCnt].Detail.IsBloodProduct;

                    var hasMismatch = (!first.HasValue && others.HasValue) || (first.HasValue && !others.HasValue) || (others != first);

                    if (hasMismatch)
                    {
                        vm.DisplayBloodProductLbl = true;
                        return;
                    }
                }
            }

            vm.NullableIsBloodProduct = first;
        }

        private void BuildIsDiluent(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm)
        {
            var first = formularies[0].Detail.IsDiluent;

            if (formularies.Count > 1)
            {
                for (int formularyCnt = 1; formularyCnt < formularies.Count; formularyCnt++)
                {
                    var others = formularies[formularyCnt].Detail.IsDiluent;

                    var hasMismatch = (!first.HasValue && others.HasValue) || (first.HasValue && !others.HasValue) || (others != first);

                    if (hasMismatch)
                    {
                        vm.DisplayDiluentLbl = true;
                        return;
                    }
                }
            }

            vm.NullableIsDiluent = first;
        }

        private void BuildPrescribable(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm)
        {
            var first = formularies[0].Detail.Prescribable;

            if (formularies.Count > 1)
            {
                for (int formularyCnt = 1; formularyCnt < formularies.Count; formularyCnt++)
                {
                    var others = formularies[formularyCnt].Detail.Prescribable;

                    var hasMismatch = (!first.HasValue && others.HasValue) || (first.HasValue && !others.HasValue) || (others != first);

                    if (hasMismatch)
                    {
                        vm.DisplayPrescribableLbl = true;
                        return;
                    }
                }
            }

            vm.NullablePrescribable = first;
        }

        private void BuildOutpatientMedicationCd(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm)
        {
            var first = formularies[0].Detail.OutpatientMedicationCd;

            if (formularies.Count > 1)
            {
                for (int formularyCnt = 1; formularyCnt < formularies.Count; formularyCnt++)
                {
                    var others = formularies[formularyCnt].Detail.OutpatientMedicationCd;

                    var hasMismatch = (!first.IsEmpty() && others.IsEmpty()) || (first.IsEmpty() && !others.IsEmpty()) || (others != first);

                    if (hasMismatch)
                    {
                        vm.DisplayOutpatientMedicationLbl = true;
                        return;
                    }
                }
            }

            vm.NullableOutpatientMedication = first.ToNullable<bool>();
        }

        private void BuildIgnoreDuplicateWarnings(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm)
        {
            var first = formularies[0].Detail.IgnoreDuplicateWarnings;

            if (formularies.Count > 1)
            {
                for (int formularyCnt = 1; formularyCnt < formularies.Count; formularyCnt++)
                {
                    var others = formularies[formularyCnt].Detail.IgnoreDuplicateWarnings;

                    var hasMismatch = (!first.IsEmpty() && others.IsEmpty()) || (first.IsEmpty() && !others.IsEmpty()) || (others != first);

                    if (hasMismatch)
                    {
                        vm.DisplayIgnoreDuplicateWarningLbl = true;
                        return;
                    }
                }
            }

            vm.NullableIgnoreDuplicateWarnings = first.ToNullable<bool>();
        }

        private void BuildIsCustomControlledDrug(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm)
        {
            var first = formularies[0].Detail.IsCustomControlledDrug;

            if (formularies.Count > 1)
            {
                for (int formularyCnt = 1; formularyCnt < formularies.Count; formularyCnt++)
                {
                    var others = formularies[formularyCnt].Detail.IsCustomControlledDrug;

                    var hasMismatch = (!first.HasValue && others.HasValue) || (first.HasValue && !others.HasValue) || (others != first);

                    if (hasMismatch)
                    {
                        vm.DisplayControlledDrugLbl = true;
                        return;
                    }
                }
            }

            vm.NullableIsCustomControlledDrug = first;
        }

        private void BuildIsPrescriptionPrintingRequired(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm)
        {
            var first = formularies[0].Detail.IsPrescriptionPrintingRequired;

            if (formularies.Count > 1)
            {
                for (int formularyCnt = 1; formularyCnt < formularies.Count; formularyCnt++)
                {
                    var others = formularies[formularyCnt].Detail.IsPrescriptionPrintingRequired;

                    var hasMismatch = (!first.HasValue && others.HasValue) || (first.HasValue && !others.HasValue) || (others != first);

                    if (hasMismatch)
                    {
                        vm.DisplayPrescriptionPrintingRequiredLbl = true;
                        return;
                    }
                }
            }

            vm.NullableIsPrescriptionPrintingRequired = first;
        }

        private void BuildIsIndicationMandatory(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm)
        {
            var first = formularies[0].Detail.IsIndicationMandatory;

            if (formularies.Count > 1)
            {
                for (int formularyCnt = 1; formularyCnt < formularies.Count; formularyCnt++)
                {
                    var others = formularies[formularyCnt].Detail.IsIndicationMandatory;

                    var hasMismatch = (!first.HasValue && others.HasValue) || (first.HasValue && !others.HasValue) || (others != first);

                    if (hasMismatch)
                    {
                        vm.DisplayIndicationMandatoryLbl = true;
                        return;
                    }
                }
            }

            vm.NullableIsIndicationMandatory = first;
        }

        private void BuildWitnessingRequired(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm)
        {
            var first = formularies[0].Detail.WitnessingRequired;

            if (formularies.Count > 1)
            {
                for (int formularyCnt = 1; formularyCnt < formularies.Count; formularyCnt++)
                {
                    var others = formularies[formularyCnt].Detail.WitnessingRequired;

                    var hasMismatch = (!first.IsEmpty() && others.IsEmpty()) || (first.IsEmpty() && !others.IsEmpty()) || (others != first);

                    if (hasMismatch)
                    {
                        vm.DisplayWitnessingRequiredLbl = true;
                        return;
                    }
                }
            }

            vm.NullableWitnessingRequired = first.ToNullable<bool>();
        }

        private void BuildRnohFormularyStatuscd(List<FormularyHeaderAPIModel> formularies, BulkFormularyEditModel vm)
        {
            var first = formularies[0].Detail.RnohFormularyStatuscd;

            if (formularies.Count > 1)
            {
                for (int formularyCnt = 1; formularyCnt < formularies.Count; formularyCnt++)
                {
                    var others = formularies[formularyCnt].Detail.RnohFormularyStatuscd;

                    var hasMismatch = (!first.IsEmpty() && others.IsEmpty()) || (first.IsEmpty() && !others.IsEmpty()) || (others != first);

                    if (hasMismatch)
                    {
                        vm.DisplayFormularyStatusLbl = true;
                        return;
                    }
                }
            }

            vm.RnohFormularyStatuscd = first;
        }
    }
}