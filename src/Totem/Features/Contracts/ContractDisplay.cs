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

            BuildDisplayText(properties, displayText, fullDetails);

            displayText = string.Join("", _displayContractRows);

            return displayText;
        }

        private static void BuildDisplayText(CaseInsensitiveDictionary<SchemaObject> properties, string displayText, bool fullDetails, int depth = 0)
        {
            foreach (var property in properties)
            {
                var type = property.Value.Type;
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

                if (type.EqualsCaseInsensitive(DataType.Array.Value))
                {
                    var itemType = property.Value.Items.Type;
                    var displayModifier = !string.IsNullOrEmpty(itemType);
                    var modifier = displayModifier ? $" ({itemType})" : "";
                    displayText = $"<span class='contract-display-row' style='--depth:{depth};'><em>{property.Key}</em> - {type}{modifier}<br></span>";
                    _displayContractRows.Add(displayText);
                }
                else if (type.EqualsCaseInsensitive("object"))
                {
                    displayText = $"<span class='contract-display-row' style='--depth:{depth};'><em>{property.Key}</em> - {type}<br></span>";
                    _displayContractRows.Add(displayText);
                    BuildDisplayText(property.Value.Properties, property.Key, fullDetails, depth + 1);
                }
                else
                {
                    var displayModifier = format != null || reference != null || pattern != "";
                    var modifier = displayModifier ? $" ({format}{reference}{pattern})" : "";
                    displayText = $"<span class='contract-display-row' style='--depth:{depth};'><em>{property.Key}</em> - {type}{modifier}<br></span>";
                    _displayContractRows.Add(displayText);
                }
            }
        }
    }
}
