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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SynapseStudioWeb.Models.MedicationMgmt.Validators
{
    public class VTMFormularyEditValidator : IFormularyEditValidator
    {
        private List<ValidationResult> _results = new List<ValidationResult>();
        private FormularyEditModel _model;

        public VTMFormularyEditValidator(FormularyEditModel model)
        {
            _model = model;
        }

        public List<ValidationResult> Validate()
        {
            if (_model == null) return _results;

            ValidateBasicInfo();

            ValidateClassificationCodes();

            ValidateIdentificationCodes();

            return _results;
        }

        private void ValidateBasicInfo()
        {
            if (!_model.IsBulkEdit)
            {
                if(_model.Name.IsEmpty())
                    _results.Add(new ValidationResult("Please enter Name in Product Details.", new List<string> { nameof(_model.Name) }));
                if (_model.Code.IsEmpty())
                    _results.Add(new ValidationResult("Please enter Code in Product Details.", new List<string> { nameof(_model.Code) }));
                if (_model.CodeSystem.IsEmpty())
                    _results.Add(new ValidationResult("Please select Code System in Product Details.", new List<string> { nameof(_model.CodeSystem) }));
                //if (_model.Status.IsEmpty())
                //    _results.Add(new ValidationResult("Please select Status in History.", new List<string> { nameof(_model.Status) }));
            }
        }

        private void ValidateClassificationCodes()
        {
            if (_model.FormularyClassificationCodes.IsCollectionValid())
            {
                var hasEmptyCodes = _model.FormularyClassificationCodes.Any(rec => rec.AdditionalCode.IsEmpty() || rec.AdditionalCodeSystem.IsEmpty() || rec.AdditionalCodeDesc.IsEmpty());

                if (hasEmptyCodes)
                {
                    _results.Add(new ValidationResult("Classification Code, Description and Classification Code System under Product Details cannot be empty.", new List<string> { nameof(_model.FormularyClassificationCodes) }));
                    return;
                }

                var addlnCodeSys = new Dictionary<string, int>();
                var hasDups = false;
                var addlnCodes = new Dictionary<string, int>();
                var hasDupCodes = false;
                var validBNF = true;

                _model.FormularyClassificationCodes.Each(rec =>
                {
                    //Same code system is displayed and hence commenting
                    //if (addlnCodeSys.ContainsKey(rec.AdditionalCodeSystem))
                    //{
                    //    hasDups = true;
                    //    addlnCodeSys[rec.AdditionalCodeSystem] = addlnCodeSys[rec.AdditionalCodeSystem] + 1;
                    //}
                    //else
                    //{
                    //    addlnCodeSys[rec.AdditionalCodeSystem] = 1;
                    //}

                    var dupKey = $"{rec.AdditionalCodeSystem}:-{rec.AdditionalCode}";
                    if (addlnCodes.ContainsKey(dupKey))
                    {
                        hasDupCodes = true;
                        addlnCodes[dupKey] = addlnCodes[dupKey] + 1;
                    }
                    else
                    {
                        addlnCodes[dupKey] = 1;
                    }

                    //bnfs should have only 7 chars
                    if (string.Compare(rec.AdditionalCodeSystem, "bnf", true) == 0 && validBNF)
                    {
                        validBNF = rec.AdditionalCode.Length == 2 || rec.AdditionalCode.Length == 4 || rec.AdditionalCode.Length == 6 || rec.AdditionalCode.Length == 7;
                    }

                });
                if (hasDups)
                {
                    _results.Add(new ValidationResult("Classification Code System under Product Details should be unique.", new List<string> { nameof(_model.FormularyClassificationCodes) }));
                }
                if (hasDupCodes)
                {
                    _results.Add(new ValidationResult("Classification Code under Product Details should be unique.", new List<string> { nameof(_model.FormularyClassificationCodes) }));
                }
                if (!validBNF)
                    _results.Add(new ValidationResult("BNF Code should be of 2, 4, 6 or 7 characters.", new List<string> { nameof(_model.FormularyClassificationCodes) }));
            }
        }

        private void ValidateIdentificationCodes()
        {
            if (_model.FormularyIdentificationCodes.IsCollectionValid())
            {
                var hasEmptyCodes = _model.FormularyIdentificationCodes.Any(rec => rec.AdditionalCode.IsEmpty() || rec.AdditionalCodeSystem.IsEmpty());

                if (hasEmptyCodes)
                {
                    _results.Add(new ValidationResult("Identification Code or Identification Code System under Product Details cannot be empty.", new List<string> { nameof(_model.FormularyIdentificationCodes) }));
                    return;
                }

                var addlnCodeSys = new Dictionary<string, int>();
                var hasDups = false;
                var addlnCodes = new Dictionary<string, int>();
                var hasDupCodes = false;

                _model.FormularyIdentificationCodes.Each(rec =>
                {
                    if (addlnCodeSys.ContainsKey(rec.AdditionalCodeSystem))
                    {
                        hasDups = true;
                        addlnCodeSys[rec.AdditionalCodeSystem] = addlnCodeSys[rec.AdditionalCodeSystem] + 1;
                    }
                    else
                    {
                        addlnCodeSys[rec.AdditionalCodeSystem] = 1;
                    }

                    if (addlnCodes.ContainsKey(rec.AdditionalCode))
                    {
                        hasDupCodes = true;
                        addlnCodes[rec.AdditionalCode] = addlnCodes[rec.AdditionalCode] + 1;
                    }
                    else
                    {
                        addlnCodes[rec.AdditionalCode] = 1;
                    }
                });
                if (hasDups)
                {
                    _results.Add(new ValidationResult("Identification Code System under Product Details should be unique.", new List<string> { nameof(_model.FormularyIdentificationCodes) }));
                }
                if (hasDupCodes)
                {
                    _results.Add(new ValidationResult("Identification Code under Product Details should be unique.", new List<string> { nameof(_model.FormularyIdentificationCodes) }));
                }
            }
        }
    }
}
