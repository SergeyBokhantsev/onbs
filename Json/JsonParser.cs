using System;
using System.Collections;
using System.IO;
using System.Text;

namespace Json
{
    public enum JsonType { Object, Field, Value, Array };

    public class JPath
    {
        // Example: "geometry.coordinates.[0].[1]"
        public static JsonEntity GetEntity(JsonObj obj, string path)
        {
            try
            {
                var items = path.Split('.');
                JsonEntity current = obj;

                foreach (var item in items)
                {
                    if (item[0] == '[')
                    {
                        if (current.Type == JsonType.Array)
                        {
                            var index = int.Parse(item.Substring(1, item.Length - 2));
                            current = ((JsonArray)current).Get(index);
                        }
                        else
                            return null;
                    }
                    else
                    {
                        if (current.Type == JsonType.Object)
                        {
                            var field = ((JsonObj)current).GetField(item);
                            if (field != null)
                                current = field.Value;
                            else
                                return null;
                        }
                        else
                            return null;
                    }
                }

                return current;
            }
            catch
            {
                return null;
            }
        }

        public static T GetFieldValue<T>(JsonObj obj, string path)
        {
            var entity = GetEntity(obj, path);

            if (!(entity is JsonValue<T>))
                return default(T);
            else
                return (entity as JsonValue<T>).Value;
        }
    }

    public abstract class JsonEntity
    {
        public JsonType Type { get; private set; }

        protected JsonEntity(JsonType type)
        {
            Type = type;
        }
    }

    public class JsonObj : JsonEntity
    {
        private ArrayList fields = new ArrayList();

        public JsonObj()
            : base(JsonType.Object)
        {
            fields = new ArrayList();
        }

        public void AddField(JsonField field)
        {
            fields.Add(field);
        }

        public JsonField GetField(string name)
        {
            foreach (var o in fields)
            {
                var field = o as JsonField;
                if (field.Name == name)
                    return field;
            }

            return null;
        }

        public override string ToString()
        {
            return "OBJECT";
        }
    }

    public class JsonField : JsonEntity
    {
        public string Name = "";
        public JsonEntity Value;

        public JsonField()
            : base(JsonType.Field)
        {
        }

        public override string ToString()
        {
            return string.Concat("FIELD '", Name, "': ", Value.ToString());
        }
    }

    public class JsonValue<T> : JsonEntity
    {
        public JsonValue(T value)
            : base(JsonType.Value)
        {
            Value = value;
        }

        public T Value { get; private set; }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class JsonArray : JsonEntity
    {
        private ArrayList values;

        public int Count
        {
            get
            {
                return values.Count;
            }
        }

        public JsonArray()
            : base(JsonType.Array)
        {
            values = new ArrayList();
        }

        public void Add(JsonEntity obj)
        {
            values.Add(obj);
        }

        public JsonEntity Get(int index)
        {
            return values[index] as JsonEntity;
        }

        public override string ToString()
        {
            return string.Concat("ARRAY [", values.Count.ToString(), "]");
        }
    }

    public class JsonParser
    {
        public static bool TryParse(byte[] data, out JsonObj result)
        {
            try
            {
                var parser = new JsonParser();
                result = parser.Parse(new MemoryStream(data));
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public JsonObj Parse(Stream stream)
        {
            int val;
            while ((val = stream.ReadByte()) != -1)
            {
                if (val == '{')
                {
                    return ParseInner(stream);
                }
            }

            throw new InvalidOperationException();
        }

        private enum PM { LookingForNameQuote, LookingForDoubleDot, LookingForValueBegin, UnquotedValueReading };

        private string ReadUntilQuote(Stream stream)
        {
            var sb = new StringBuilder();

            int val;
            while ((val = stream.ReadByte()) != -1)
            {
                var symb = (char)val;

                if (symb != '"')
                    sb.Append(symb);
                else
                    return sb.ToString();
            }

            throw new InvalidOperationException();
        }

        private string ReadUntilComma(char first, Stream stream)
        {
            var sb = new StringBuilder(first.ToString());

            int val;
            while ((val = stream.ReadByte()) != -1)
            {
                var symb = (char)val;

                if (symb != ',')
                    sb.Append(symb);
                else
                    return sb.ToString().Trim();
            }

            throw new InvalidOperationException();
        }

        private JsonObj ParseInner(Stream stream)
        {
            var mode = PM.LookingForNameQuote;

            JsonObj obj = new JsonObj();

            JsonField current = null;

            string stringValue = string.Empty;

            int val;
            while ((val = stream.ReadByte()) != -1)
            {
                var symb = (char)val;

                switch (mode)
                {
                    case PM.LookingForNameQuote:
                        switch (symb)
                        {
                            case '"':
                                current = new JsonField();
                                current.Name = ReadUntilQuote(stream);
                                mode++;
                                break;
                            case '}':
                                return obj;
                        }
                        break;

                    case PM.LookingForDoubleDot:
                        switch (symb)
                        {
                            case ':':
                                mode++;
                                break;
                        }
                        break;

                    case PM.LookingForValueBegin:
                        switch (symb)
                        {
                            case '"':
                                current.Value = new JsonValue<string>(ReadUntilQuote(stream));
                                obj.AddField(current);
                                mode = PM.LookingForNameQuote;
                                break;
                            case '{':
                                current.Value = ParseInner(stream);
                                obj.AddField(current);
                                mode = PM.LookingForNameQuote;
                                break;

                            case '[':
                                current.Value = ParseArray(stream, current.Name);
                                obj.AddField(current);
                                mode = PM.LookingForNameQuote;
                                break;

                            case ' ':
                            case '\r':
                            case '\n':
                                break;

                            default:
                                stringValue = string.Empty;
                                stringValue += symb;
                                mode++;
                                break;
                        }
                        break;

                    case PM.UnquotedValueReading:
                        switch (symb)
                        {
                            case ',':
                                current.Value = CreateValue(stringValue.Trim());
                                obj.AddField(current);
                                mode = PM.LookingForNameQuote;
                                break;

                            case '}':
                                current.Value = CreateValue(stringValue.Trim());
                                obj.AddField(current);
                                return obj;

                            default:
                                stringValue += symb;
                                break;
                        }
                        break;
                }
            }

            throw new InvalidOperationException();
        }

        private JsonEntity CreateValue(string rawValue)
        {
            bool @bool;
            int @int;
            double @double;

            if (bool.TryParse(rawValue, out @bool))
                return new JsonValue<bool>(@bool);
            else if (int.TryParse(rawValue, out @int))
                return new JsonValue<int>(@int);
            else if (double.TryParse(rawValue, out @double))
                return new JsonValue<double>(@double);
            else
                throw new InvalidDataException(string.Format("Unable to create Json value from '{0}'", rawValue));
        }

        private JsonArray ParseArray(Stream stream, string name)
        {
            var ret = new JsonArray();

            string item = string.Empty;

            int val;
            while ((val = stream.ReadByte()) != -1)
            {
                var symb = (char)val;

                switch (symb)
                {
                    case '{':
                        ret.Add(ParseInner(stream));
                        break;
                    case '[':
                        ret.Add(ParseArray(stream, name + "_inner"));
                        item = string.Empty;
                        break;

                    case '"':
                        ret.Add(new JsonValue<string>(ReadUntilQuote(stream)));
                        item = string.Empty;
                        break;

                    case ',':
                        if (item != string.Empty)
                        {
                            ret.Add(CreateValue(item.Trim()));
                            item = string.Empty;
                        }
                        break;

                    case ']':
                        if (item != string.Empty)
                        {
                            ret.Add(CreateValue(item.Trim()));
                        }
                        return ret;

                    case ' ':
                    case '\r':
                    case '\n':
                        break;

                    default:
                        item += symb;
                        break;
                }

            }

            throw new InvalidOperationException();
        }
    }
}
