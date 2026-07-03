using System.Collections.Generic;
using System.Text;
using ArSpacePlanner.Measure;

namespace ArSpacePlanner.Planner
{
    /// <summary>
    /// Builds a human-readable plan report (Turkish) from the current measurements and
    /// placed furniture. Used both for the on-screen report card and the shared text.
    /// </summary>
    public static class ReportBuilder
    {
        public static string Build(MeasurementManager measure, FurniturePlacer planner, MeasureUnit unit)
        {
            var sb = new StringBuilder();
            sb.AppendLine("PLAN RAPORU");
            sb.AppendLine(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
            sb.AppendLine();

            sb.AppendLine("Olcum");
            if (measure != null && measure.PointCount >= 2)
            {
                sb.AppendLine("  Toplam uzunluk: " + Units.FormatLength(measure.TotalLength, unit));
                if (measure.AreaClosed)
                {
                    sb.AppendLine("  Kapali alan: " + Units.FormatArea(measure.Area, unit));
                }
            }
            else
            {
                sb.AppendLine("  (olcum yok)");
            }
            sb.AppendLine();

            sb.AppendLine("Esyalar");
            var counts = new Dictionary<string, int>();
            if (planner != null)
            {
                foreach (PlacedFurniture item in planner.Placed)
                {
                    if (item == null)
                    {
                        continue;
                    }
                    FurnitureDefinition def = FurnitureCatalog.ById(item.DefinitionId);
                    string name = def != null ? def.DisplayName : item.DefinitionId;
                    counts.TryGetValue(name, out int c);
                    counts[name] = c + 1;
                }
            }

            if (counts.Count == 0)
            {
                sb.AppendLine("  (esya yok)");
            }
            else
            {
                foreach (KeyValuePair<string, int> pair in counts)
                {
                    sb.AppendLine("  " + pair.Key + " x" + pair.Value);
                }
            }

            return sb.ToString();
        }
    }
}
