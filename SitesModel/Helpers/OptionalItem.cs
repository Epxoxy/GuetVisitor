using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitesModel.Helpers
{
    public class OptionalItem<T>
    {
        public string Display { get; set; }
        public T Value { get; set; }

        public OptionalItem(string display, T value)
        {
            Display = display;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Display}[{Value}]";
        }
    }

    public enum OptionalType
    {
        Input,
        Selection,
        EditableSelection,
    }

    public class Optional<T>
    {
        public string Description { get; set; }
        public OptionalType Type { get; set; }
        public OptionalItem<T>[] Items { get; set; }
    }

    public class OptionalWithValue<T>
    {
        public string Description { get; set; }
        public OptionalType Type { get; set; }
        public OptionalItem<T>[] Items { get; set; }
        public T Value { get; set; }

        public static OptionalWithValue<T> FromOptional(Optional<T> optional)
        {
            return new OptionalWithValue<T>()
            {
                Description = optional.Description,
                Type = optional.Type,
                Items = optional.Items
            };
        }
    }
}
