using System;

namespace MyCodeExample
{
    public class AlreadyRegisteredException : Exception
    {
        public AlreadyRegisteredException(System.Type T) : base(
            $"A object of that type ({T}) has already been registered!")
        {
        }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(System.Type T) : base("Object (" + T.ToString() +
                                                       ") not found.\nAlways Register() in Awake(). Never Find() in Awake(). Check Script Execution Order.")
        {
        }
    }
}