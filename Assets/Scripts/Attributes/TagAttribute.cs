using System;

namespace BML.Scripts.Attributes {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class TagAttribute : Attribute {

        public string Filter;

        public TagAttribute(string filter = null) {
            this.Filter = filter;
        }
    }
}