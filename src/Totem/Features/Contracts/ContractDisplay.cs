using System.Collections.Generic;
using Totem.Infrastructure;
using Totem.Models;

namespace Totem.Features.Contracts
{
    public class ContractDisplay
    {
        private static List<string> _displayContractRows;
        public static string ContractDetails(CaseInsensitiveDictionary<SchemaObject> schema, bool fullDetails = false)
        {
            _displayContractRows = new List<string>();
            var displayText = "";
            if (!schema.ContainsKey("Contract")) return displayText;

            var properties = schema["Contract"].Properties;

            BuildDisplayText(properties, fullDetails);

            displayText = string.Join("", _displayContractRows);

            return displayText;
        }

        private static void BuildDisplayText(CaseInsensitiveDictionary<SchemaObject> properties, bool fullDetails, int depth = 0)
        {
            foreach (var property in properties)
            {
                var dataType = property.Value.GetDataType();
                var format = property.Value.Format;
                var reference = property.Value.Reference;
                var pattern = property.Value.Pattern;

                if (pattern != null && fullDetails)
                {
                    if (format != null || reference != null)
                    {
                        pattern = $"; Pattern: { pattern}";
                    }
                    else
                    {
                        pattern = $"Pattern: { pattern}";
                    }
                }
                else
                {
                    pattern = "";
                }

                string displayText;
                if (dataType == DataType.Array)
                {
                    var itemType = property.Value.Items.Type;
                    var itemFormat = property.Value.Items.Format;
                    var itemReference = property.Value.Items.Reference;
                    var displayModifier = !string.IsNullOrEmpty(itemFormat) || !string.IsNullOrEmpty(itemType) || !string.IsNullOrEmpty(itemReference);
                    var modifier = "";

                    if (displayModifier)
                    {
                        var itemTypeString = itemType;
                        if (!string.IsNullOrEmpty(itemFormat))
                        {
                            itemTypeString = itemFormat;
                        }
                        if (!string.IsNullOrEmpty(itemReference))
                        {
                            itemTypeString = itemReference;
                        }

                        modifier = $" ({itemTypeString})";
                    }
                    
                    displayText = $"<span class='contract-display-row' style='--depth:{depth};'><em>{property.Key}</em> - {dataType.Value}{modifier}<br></span>";
                    _displayContractRows.Add(displayText);
                }
                else if (dataType == DataType.Object)
                {
                    displayText = $"<span class='contract-display-row' style='--depth:{depth};'><em>{property.Key}</em> - {dataType.Value}<br></span>";
                    _displayContractRows.Add(displayText);
                    BuildDisplayText(property.Value.Properties, fullDetails, depth + 1);
                }
                else
                {
                    var displayModifier = format != null || reference != null || pattern != "";
                    var modifier = displayModifier ? $" ({format}{reference}{pattern})" : "";
                    displayText = $"<span class='contract-display-row' style='--depth:{depth};'><em>{property.Key}</em> - {dataType.Value}{modifier}<br></span>";
                    _displayContractRows.Add(displayText);
                }
            }
        }
    }
}
