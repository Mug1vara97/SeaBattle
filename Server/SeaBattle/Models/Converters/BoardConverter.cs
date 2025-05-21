using System.Text.Json;
using System.Text.Json.Serialization;
using SeaBattle.Models;

namespace SeaBattle.Models.Converters
{
    public class BoardConverter : JsonConverter<CellState[,]>
    {
        public override CellState[,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException();
            }

            var rows = new List<List<CellState>>();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType != JsonTokenType.StartArray)
                {
                    throw new JsonException();
                }

                var row = new List<CellState>();
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    var value = JsonSerializer.Deserialize<int>(ref reader, options);
                    row.Add((CellState)value);
                }
                rows.Add(row);
            }

            var board = new CellState[rows.Count, rows[0].Count];
            for (int i = 0; i < rows.Count; i++)
            {
                for (int j = 0; j < rows[i].Count; j++)
                {
                    board[i, j] = rows[i][j];
                }
            }

            return board;
        }

        public override void Write(Utf8JsonWriter writer, CellState[,] value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            for (int i = 0; i < value.GetLength(0); i++)
            {
                writer.WriteStartArray();
                for (int j = 0; j < value.GetLength(1); j++)
                {
                    writer.WriteNumberValue((int)value[i, j]);
                }
                writer.WriteEndArray();
            }
            writer.WriteEndArray();
        }
    }
} 