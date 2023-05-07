namespace Animal_Hotel
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class ActionMapperAttribute : Attribute
    {
        public string ActionName { get; private set; }
        public string ControllerName { get; private set; }
        public string DisplayName { get; private set; }

        public ActionMapperAttribute(string actionName, string controllerName, string displayName)
        {
            ActionName = actionName;
            ControllerName = controllerName;
            DisplayName = displayName;
        }   
    }
}
