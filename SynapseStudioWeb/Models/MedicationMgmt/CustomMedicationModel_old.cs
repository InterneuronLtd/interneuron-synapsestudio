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
﻿//using Interneuron.Common.Extensions;
//using Newtonsoft.Json;
//using SynapseStudioWeb.DataService.APIModel;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.Linq;
//using System.Text;
//using System.Text.Json.Serialization;
//using System.Threading.Tasks;

//namespace SynapseStudioWeb.Models.MedicinalMgmt
//{
//    public class CustomMedicationModel_old : IValidatableObject
//    {
//        public string FormularyVersionId { get; set; }
//        public bool IsReadOnly { get; set; }
//        public bool IsImported { get; set; }
//        public string ControlIdentifier { get; set; }
//        [Required(ErrorMessage = "Please enter Name in Product Details")]
//        public string Name { get; set; }
//        [Required(ErrorMessage = "Please select Product Type in Product Details")]
//        public string ProductType { get; set; }
//        public string MedicationType { get; set; }
//        [Required(ErrorMessage = "Please enter Code in Product Details")]
//        public string Code { get; set; }
//        [Required(ErrorMessage = "Please enter Code System in Product Details")]
//        public string CodeSystem { get; set; }
//        public List<AdditionalCodeModel> FormularyAdditionalCodes { get; set; }
//        public string ATCCode { get; set; }
//        [Required(ErrorMessage = "Please select Status in History")]
//        public string Status { get; set; }
//        public string OriginalStatus { get; set; }
//        public string History { get; set; }
//        public string Id { get; set; }
//        [Required(ErrorMessage = "Please select Formulary Status in Preferences")]
//        public string FormularyStatus { get; set; }
//        [Required(ErrorMessage = "Please select Orderable in Flags / Classification")]
//        public string Orderable { get; set; }
//        public bool InpatientMedication { get; set; }
//        public bool OutpatientMedication { get; set; }
//        public string PrescribingStatus { get; set; }
//        public string Rules { get; set; }

//        public string RecStatuschangeMsg { get; set; }
//        public string RecSource { get; set; }
//        public bool UnlicensedMedication { get; set; }
//        public bool NotForPRN { get; set; }
//        public bool HighAlertMedication { get; set; }
//        public bool IgnoreDuplicateWarnings { get; set; }
//        public List<CodeNameSelectorModel> Route { get; set; }
//        public List<CodeNameSelectorModel> AdditionalRoute { get; set; }
//        public List<CodeNameSelectorModel> UnlicensedRoute { get; set; }
//        public List<CodeNameSelectorModel> DiscretionaryRoutes { get; set; }
//        public string MedusaPreparationInstructions { get; set; }
//        public List<CodeNameSelectorModel> Indication { get; set; }
//        public string Class { get; set; }
//        public bool CriticalDrug { get; set; }
//        public string ControlledDrugCategory { get; set; }
//        public bool Cytotoxic { get; set; }
//        public bool ClinicalTrialMedication { get; set; }
//        public bool Fluid { get; set; }
//        public bool Antibiotic { get; set; }
//        public bool Anticoagulant { get; set; }
//        public bool Antipsychotic { get; set; }
//        public bool Antimicrobial { get; set; }
//        public bool ReviewReminder { get; set; }
//        public bool IVToOral { get; set; }
//        public string TitrationType { get; set; }
//        public string RoundingFactor { get; set; }
//        public string MaximumDoseNumerator { get; set; }
//        public CodeNameSelectorModel MaximumDoseUnit { get; set; }
//        public bool WitnessingRequired { get; set; }
//        public bool RestrictedPrescribing { get; set; }
//        public string NotesForRestriction { get; set; }
//        public List<CodeNameSelectorModel> Cautions { get; set; }
//        public List<CodeNameSelectorModel> CustomWarning { get; set; }
//        public List<CodeNameSelectorModel> ContraIndications { get; set; }
//        public List<CodeNameSelectorModel> SideEffects { get; set; }
//        public List<CodeNameSelectorModel> SafetyMessages { get; set; }
//        public List<CodeNameSelectorModel> Endorsements { get; set; }
//        public string NICETA { get; set; }
//        public string VirtualTherapeuticMoiety { get; set; }
//        public List<CodeNameSelectorModel> LicensedUse { get; set; }
//        public List<CodeNameSelectorModel> UnlicensedUse { get; set; }
//        public string MarkedModifier { get; set; }
//        public bool Insulins { get; set; }
//        public bool MentalHealthDrug { get; set; }
//        public string BasisOfPreferredName { get; set; }
//        public bool SugarFree { get; set; }
//        public bool GlutenFree { get; set; }
//        public bool PreservativeFree { get; set; }
//        public bool CFCFree { get; set; }
//        public string DoseForm { get; set; }
//        public decimal? UnitDoseFormSize { get; set; }
//        public CodeNameSelectorModel UnitDoseFormUnits { get; set; }
//        public CodeNameSelectorModel UnitDoseUnitOfMeasure { get; set; }
//        public List<IngredientFormularyModel> Ingredients { get; set; }
//        public List<CodeNameSelectorModel> FormNRoute { get; set; }
//        public CodeNameSelectorModel FormCd { get; set; }
//        public CodeNameSelectorModel TradeFamily { get; set; }
//        public string OrderFormType { get; set; }
//        public string VirtualMedicinalProduct { get; set; }
//        public string ModifiedRelease { get; set; }
//        public bool BlackTriangle { get; set; }
//        public CodeNameSelectorModel Supplier { get; set; }
//        public string CurrentLicensingAuthority { get; set; }
//        public bool EMAAdditionalMonitoring { get; set; }
//        public bool ParallelImport { get; set; }
//        public string RestrictionsOnAvailability { get; set; }
//        public bool IsRecordStatusReadOnly { get; set; }
//        public bool IsSaveSuccessful { get; set; }
//        public string ParentCode { get; set; }
//        public string ParentName { get; set; }
//        public string ParentProductType { get; set; }
//        public bool? IsDuplicate { get; set; }
//        public string DuplicateOfFormularyId { get; set; }
//        public bool ExpensiveMedication { get; set; }

//        //public Dictionary<string, FormularyLabelControlModel> LabelControlMetaDetails { get; set; }

//        public Dictionary<string, bool> ControlDisplayMetaDetails { get; set; }
//        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
//        {
//            if (Status == "004")
//            {
//                if (RecStatuschangeMsg == null || RecStatuschangeMsg.IsEmpty())
//                {
//                    yield return new ValidationResult("Please Enter Reason", new[] { nameof(RecStatuschangeMsg) });
//                }
//            }
//            if (ProductType != null && string.Compare(ProductType, "amp", true) == 0)
//            {
//                if (Supplier == null || Supplier.Id.IsEmpty())
//                {
//                    yield return new ValidationResult("Supplier (Product Details) is mandatory for AMP", new[] { nameof(Supplier) });
//                }

//                if (FormularyAdditionalCodes.IsCollectionValid())
//                {
//                    foreach (var additionalCode in FormularyAdditionalCodes)
//                    {
//                        if ((additionalCode.AdditionalCode == null || additionalCode.AdditionalCode.IsEmpty()) && (additionalCode.AdditionalCodeSystem == null || additionalCode.AdditionalCodeSystem.IsEmpty()))
//                        {
//                            yield return new ValidationResult("Please enter Additional Codes in Product Details Section", new[] { nameof(additionalCode.AdditionalCode) });
//                        }

//                        if ((additionalCode.AdditionalCode == null || additionalCode.AdditionalCode.IsEmpty()) && (additionalCode.AdditionalCodeSystem != null || additionalCode.AdditionalCodeSystem.IsNotEmpty()))
//                        {
//                            yield return new ValidationResult("Please enter Additional Code in Product Details Section", new[] { nameof(additionalCode.AdditionalCode) });
//                        }

//                        if ((additionalCode.AdditionalCodeSystem == null || additionalCode.AdditionalCodeSystem.IsEmpty()) && (additionalCode.AdditionalCode != null || additionalCode.AdditionalCode.IsNotEmpty()))
//                        {
//                            yield return new ValidationResult("Please enter Additional Code System in Product Details Section", new[] { nameof(additionalCode.AdditionalCodeSystem) });
//                        }
//                    }
//                }

//                if (Ingredients.IsCollectionValid())
//                {
//                    foreach (var ingredient in Ingredients)
//                    {
//                        if (ingredient.Ingredient == null || ingredient.Ingredient.Id.IsEmpty())
//                        {
//                            yield return new ValidationResult("Please enter Ingredient in Posology Section", new[] { nameof(ingredient.Ingredient) });
//                        }

//                        if (ingredient.StrengthValNumerator == null || ingredient.StrengthValNumerator.IsEmpty())
//                        {
//                            yield return new ValidationResult("Please enter Strength Value Numerator in Posology section", new[] { nameof(ingredient.StrengthValNumerator) });
//                        }

//                        if (ingredient.StrengthValueNumeratorUnit == null || ingredient.StrengthValueNumeratorUnit.Id.IsEmpty())
//                        {
//                            yield return new ValidationResult("Please enter Strength Value Numerator Unit in Posology section", new[] { nameof(ingredient.StrengthValueNumeratorUnit) });
//                        }

//                        if ((ingredient.StrengthValDenominator == null || ingredient.StrengthValDenominator.IsEmpty()) && (ingredient.StrengthValueDenominatorUnit != null && ingredient.StrengthValueDenominatorUnit.Id.IsNotEmpty()))
//                        {
//                            yield return new ValidationResult("Please enter Strength Value Denominator in Posology section", new[] { nameof(ingredient.StrengthValDenominator) });
//                        }

//                        if ((ingredient.StrengthValueDenominatorUnit == null || ingredient.StrengthValueDenominatorUnit.Id.IsEmpty()) && (ingredient.StrengthValDenominator != null || ingredient.StrengthValDenominator.IsNotEmpty()))
//                        {
//                            yield return new ValidationResult("Please enter Strength Value Denominator Unit in Posology section", new[] { nameof(ingredient.StrengthValueDenominatorUnit) });
//                        }
//                    }
//                }
//            }

//            if (ProductType != null && string.Compare(ProductType, "vtm", true) == 0)
//            {
//                if (FormularyAdditionalCodes.IsCollectionValid())
//                {
//                    foreach (var additionalCode in FormularyAdditionalCodes)
//                    {
//                        if ((additionalCode.AdditionalCode == null || additionalCode.AdditionalCode.IsEmpty()) && (additionalCode.AdditionalCodeSystem == null || additionalCode.AdditionalCodeSystem.IsEmpty()))
//                        {
//                            yield return new ValidationResult("Please enter Additional Codes in Product Details Section", new[] { nameof(additionalCode.AdditionalCode) });
//                        }

//                        if ((additionalCode.AdditionalCode == null || additionalCode.AdditionalCode.IsEmpty()) && (additionalCode.AdditionalCodeSystem != null || additionalCode.AdditionalCodeSystem.IsNotEmpty()))
//                        {
//                            yield return new ValidationResult("Please enter Additional Code in Product Details Section", new[] { nameof(additionalCode.AdditionalCode) });
//                        }

//                        if ((additionalCode.AdditionalCodeSystem == null || additionalCode.AdditionalCodeSystem.IsEmpty()) && (additionalCode.AdditionalCode != null || additionalCode.AdditionalCode.IsNotEmpty()))
//                        {
//                            yield return new ValidationResult("Please enter Additional Code System in Product Details Section", new[] { nameof(additionalCode.AdditionalCodeSystem) });
//                        }
//                    }
//                }


//                if (Ingredients.IsCollectionValid())
//                {
//                    foreach (var ingredient in Ingredients)
//                    {
//                        if (ingredient.Ingredient == null || ingredient.Ingredient.Id.IsEmpty())
//                        {
//                            yield return new ValidationResult("Please enter Ingredient in Posology Section", new[] { nameof(ingredient.Ingredient) });
//                        }

//                        if (ingredient.StrengthValNumerator == null || ingredient.StrengthValNumerator.IsEmpty())
//                        {
//                            yield return new ValidationResult("Please enter Strength Value Numerator in Posology section", new[] { nameof(ingredient.StrengthValNumerator) });
//                        }

//                        if (ingredient.StrengthValueNumeratorUnit == null || ingredient.StrengthValueNumeratorUnit.Id.IsEmpty())
//                        {
//                            yield return new ValidationResult("Please enter Strength Value Numerator Unit in Posology section", new[] { nameof(ingredient.StrengthValueNumeratorUnit) });
//                        }

//                        if ((ingredient.StrengthValDenominator == null || ingredient.StrengthValDenominator.IsEmpty()) && (ingredient.StrengthValueDenominatorUnit != null && ingredient.StrengthValueDenominatorUnit.Id.IsNotEmpty()))
//                        {
//                            yield return new ValidationResult("Please enter Strength Value Denominator in Posology section", new[] { nameof(ingredient.StrengthValDenominator) });
//                        }

//                        if ((ingredient.StrengthValueDenominatorUnit == null || ingredient.StrengthValueDenominatorUnit.Id.IsEmpty()) && (ingredient.StrengthValDenominator != null || ingredient.StrengthValDenominator.IsNotEmpty()))
//                        {
//                            yield return new ValidationResult("Please enter Strength Value Denominator Unit in Posology section", new[] { nameof(ingredient.StrengthValueDenominatorUnit) });
//                        }
//                    }
//                }
//            }

//            if (ProductType != null && string.Compare(ProductType, "vmp", true) == 0)
//            {
//                if (RecSource != "I")
//                {
//                    if (!Ingredients.IsCollectionValid())
//                    {
//                        yield return new ValidationResult("Ingredient (Posology) is mandatory for VMP", new[] { nameof(Ingredients) });
//                    }
//                    else if (Ingredients.IsCollectionValid())
//                    {
//                        foreach (var ingredient in Ingredients)
//                        {
//                            if (ingredient.Ingredient == null || ingredient.Ingredient.Id.IsEmpty())
//                            {
//                                yield return new ValidationResult("Ingredient (Posology) is mandatory for VMP", new[] { nameof(ingredient.Ingredient) });
//                            }

//                            if (ingredient.StrengthValNumerator == null || ingredient.StrengthValNumerator.IsEmpty())
//                            {
//                                yield return new ValidationResult("Strength Value Numerator (Posology) is mandatory for VMP", new[] { nameof(ingredient.StrengthValNumerator) });
//                            }

//                            if (ingredient.StrengthValueNumeratorUnit == null || ingredient.StrengthValueNumeratorUnit.Id.IsEmpty())
//                            {
//                                yield return new ValidationResult("Strength Value Numerator Unit (Posology) is mandatory for VMP", new[] { nameof(ingredient.StrengthValueNumeratorUnit) });
//                            }
//                        }
//                    }

//                    if (!Route.IsCollectionValid())
//                    {
//                        yield return new ValidationResult("Route (Posology) is mandatory for VMP", new[] { nameof(Route) });
//                    }
//                    else if (Route.IsCollectionValid())
//                    {
//                        foreach (var route in Route)
//                        {
//                            if (route == null || route.Id.IsEmpty())
//                            {
//                                yield return new ValidationResult("Route (Posology) is mandatory for VMP", new[] { nameof(Route) });
//                            }
//                        }
//                    }

//                    if (!FormNRoute.IsCollectionValid())
//                    {
//                        yield return new ValidationResult("Form & Route (Posology) is mandatory for VMP", new[] { nameof(FormNRoute) });
//                    }
//                    else if (FormNRoute.IsCollectionValid())
//                    {
//                        foreach (var formAndRoute in FormNRoute)
//                        {
//                            if (formAndRoute == null || formAndRoute.Id.IsEmpty())
//                            {
//                                yield return new ValidationResult("Form & Route (Posology) is mandatory for VMP", new[] { nameof(FormNRoute) });
//                            }
//                        }
//                    }

//                    if (FormCd == null || FormCd.Id.IsEmpty())
//                    {
//                        yield return new ValidationResult("Form (Posology) is mandatory for VMP", new[] { nameof(FormCd) });
//                    }

//                    if (!UnitDoseFormSize.HasValue)
//                    {
//                        yield return new ValidationResult("Unit Dose Form Size (Posology) is mandatory for VMP", new[] { nameof(UnitDoseFormSize) });
//                    }

//                    if (UnitDoseFormUnits == null || UnitDoseFormUnits.Id.IsEmpty())
//                    {
//                        yield return new ValidationResult("Unit Dose Form Units (Posology) is mandatory for VMP", new[] { nameof(UnitDoseFormUnits) });
//                    }

//                    if (UnitDoseUnitOfMeasure == null || UnitDoseUnitOfMeasure.Id.IsEmpty())
//                    {
//                        yield return new ValidationResult("Unit Dose Unit Of Measure (Posology) is mandatory for VMP", new[] { nameof(UnitDoseUnitOfMeasure) });
//                    }

//                    if (ControlledDrugCategory == null || ControlledDrugCategory.IsEmpty())
//                    {
//                        yield return new ValidationResult("Controlled Drug Category (Flags / Classification) is mandatory for VMP", new[] { nameof(ControlledDrugCategory) });
//                    }
//                }

//                if (DoseForm == null || DoseForm.IsEmpty())
//                {
//                    yield return new ValidationResult("Dose Form (Posology) is mandatory for VMP", new[] { nameof(DoseForm) });
//                }
//            }
//        }
//    }


//    public class CodeNameSelectorModel
//    {
//        [JsonProperty("id")]
//        public string Id { get; set; }
//        [JsonProperty("name")]
//        public string Name { get; set; }
//        [JsonProperty("readonly")]
//        public bool IsReadonly { get; set; } = false;
//        [JsonProperty("datasource")]
//        public string Source { get; set; } = string.Empty;

//        [JsonProperty("color")]
//        public string SourceColor { get; set; }
//    }

//    public class IngredientFormularyModel
//    {
//        public string FormularyVersionId { get; set; }
//        public CodeNameSelectorModel Ingredient { get; set; }
//        public string BasisOfPharmaceuticalStrength { get; set; }
//        public string StrengthValNumerator { get; set; }
//        public CodeNameSelectorModel StrengthValueNumeratorUnit { get; set; }
//        public string StrengthValDenominator { get; set; }
//        public CodeNameSelectorModel StrengthValueDenominatorUnit { get; set; }

//    }
//    public class AdditionalCodeModel
//    {
//        public string FormularyVersionId { get; set; }
//        public string AdditionalCode { get; set; }
//        public string AdditionalCodeSystem { get; set; }
//        public string AdditionalCodeDesc { get; set; }
//        public string Attr1 { get; set; }
//        public string MetaJson { get; set; }
//        public string Source { get; set; }
//        public string CodeType { get; set; }

//        public string AdditionalProps
//        {
//            get
//            {
//                var addnlProps = new StringBuilder();

//                if (AdditionalCodeDesc.IsNotEmpty())
//                    addnlProps.Append($"Description: {AdditionalCodeDesc}<br/>");
//                if (Source.IsNotEmpty())
//                    addnlProps.Append($"Source: {Source}<br/>");

//                if (MetaJson.IsNotEmpty())
//                {
//                    try
//                    {
//                        var metaDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(MetaJson);
//                        metaDict.Each((kv) =>
//                        {
//                            addnlProps.Append($"{kv.Key}: {kv.Value}<br/>");
//                        });

//                    }
//                    catch { }
//                }

//                return addnlProps.ToString();
//            }
//        }
//    }
//}



