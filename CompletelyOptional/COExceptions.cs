using CompletelyOptional;
using System;

namespace OptionalUI
{
    /// <summary>
    /// Thrown when ModID cannot be used as Folder Name
    /// </summary>
    [Serializable]
    public class InvalidModNameException : FormatException
    {
        public InvalidModNameException(string modID) : base(string.Concat(modID, " is invalid ModID! Use something that can be used as folder name!"))
        {
        }

        public InvalidModNameException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InvalidModNameException()
        {
        }

        protected InvalidModNameException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Exception that's thrown when your mod tried to access MenuObject in UIelement outside Mod Config Screen
    /// </summary>
    [Serializable]
    public class InvalidMenuObjAccessException : NullReferenceException
    {
        public InvalidMenuObjAccessException(string message) : base(message)
        {
        }

        public InvalidMenuObjAccessException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InvalidMenuObjAccessException() : base("If you are accessing MenuObject in UIelements, make sure those don't run when \'isOptionMenu\' is \'false\'.")
        {
        }

        public InvalidMenuObjAccessException(Exception ex) : base(string.Concat("If you are accessing MenuObject in UIelements, make sure those don't run when \'isOptionMenu\' is \'false\'.", Environment.NewLine, ex.ToString()))
        {
        }

        protected InvalidMenuObjAccessException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// You need at least one OpTab to contain any UIelements
    /// </summary>
    [Serializable]
    public class NoTabException : FormatException
    {
        public NoTabException(string modID) : base(string.Concat("NoTabException: ", modID, " OI has No OpTabs! ",
                        Environment.NewLine, "Did you put base.Initialize() after your code?",
                        Environment.NewLine, "Leaving OI.Initialize() completely blank will prevent the mod from using LoadData/SaveData."
                        ))
        {
        }

        public NoTabException()
        {
        }

        public NoTabException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoTabException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// You need at least one OpTab to contain any UIelements
    /// </summary>
    [Serializable]
    public class GeneralInitializeException : FormatException
    {
        public GeneralInitializeException(Exception ex) : base(string.Concat("GeneralInitializeException: OI had a problem in Initialize!",
                        Environment.NewLine, InternalTranslator.Translate("This issue might be resolved by downloading the latest ConfigMachine."),
                        Environment.NewLine, ex
                        ))
        {
        }

        public GeneralInitializeException(string message) : base(string.Concat("GeneralInitializeException: OI had a problem in Initialize!",
                        Environment.NewLine, InternalTranslator.Translate("This issue might be resolved by downloading the latest ConfigMachine."),
                        Environment.NewLine, message
                        ))
        {
        }

        public GeneralInitializeException()
        {
        }

        public GeneralInitializeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected GeneralInitializeException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// There's already a UIconfig with same key
    /// </summary>
    [Serializable]
    public class DupelicateKeyException : FormatException
    {
        public DupelicateKeyException(string tab, string key) : base(string.Concat(string.IsNullOrEmpty(tab) ? "Tab" : "Tab ", tab, " has duplicated key for UIconfig.",
                          Environment.NewLine, "(dupe key: ", key, ")"))
        {
        }

        public DupelicateKeyException()
        {
        }

        public DupelicateKeyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DupelicateKeyException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }

        public DupelicateKeyException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Problem occured in Option Menu Update
    /// </summary>
    [Serializable]
    public class GenericUpdateException : ApplicationException
    {
        public GenericUpdateException()
        {
        }

        public GenericUpdateException(string log) : base(log)
        {
        }

        public GenericUpdateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected GenericUpdateException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Problem occured in LoadData
    /// </summary>
    [Serializable]
    public class LoadDataException : Exception
    {
        public LoadDataException(string message) : base(message)
        {
        }

        public LoadDataException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public LoadDataException()
        {
        }

        protected LoadDataException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Problem occured in SaveData
    /// </summary>
    [Serializable]
    public class SaveDataException : Exception
    {
        public SaveDataException(string message) : base(message)
        {
        }

        public SaveDataException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public SaveDataException()
        {
        }

        protected SaveDataException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Wrong argument for (usually) constructor in <see cref="UIelement"/>
    /// </summary>
    [Serializable]
    public class ElementFormatException : ArgumentException
    {
        public ElementFormatException(UIelement element, string message, string key = "") : base(
            string.Concat(element.GetType().Name, " threw exception : ", message, string.IsNullOrEmpty(key) ? string.Empty : string.Concat(" (Key : ", key, ")"))
            )
        {
        }

        public ElementFormatException(string message) : base(string.Concat("Invalid argument for UIelement ctor : ", message))
        {
        }

        public ElementFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ElementFormatException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
        }

        public ElementFormatException() : base("One of UIelement threw exception for Invalid arguments!")
        {
        }
    }

    /// <summary>
    /// Called progData property without being it progData
    /// </summary>
    [Serializable]
    public class InvalidGetPropertyException : FormatException
    {
        public InvalidGetPropertyException(OptionInterface oi, string name) : base(
            string.Concat($"{oi.rwMod.ModID} called {name} eventhough its progData is false!"))
        {
        }

        public InvalidGetPropertyException(UIelement element, string name) : base(
            string.Concat(element, element is UIconfig ? string.Concat($"(key: {(element as UIconfig).key})") : string.Empty,
                $" called {name} which is Invalid!"))
        {
        }

        public InvalidGetPropertyException(string message) : base(string.Concat("NoProgDataException: ", message))
        {
        }

        public InvalidGetPropertyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InvalidGetPropertyException() : base("Invalid property called eventhough its progData is false")
        {
        }

        protected InvalidGetPropertyException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
        }
    }

    /// <summary>
    /// Wrong method called for wrong <see cref="UIelement"/>
    /// </summary>
    [Serializable]
    public class InvalidActionException : InvalidOperationException
    {
        public InvalidActionException(string message) : base(message)
        {
        }

        public InvalidActionException(UIelement element, string message, string key = "") : base(
            string.Concat(element.GetType().Name, " threw exception : ", message, string.IsNullOrEmpty(key) ? string.Empty : string.Concat(" (Key : ", key, ")")))
        {
        }

        public InvalidActionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InvalidActionException()
        {
        }

        protected InvalidActionException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// <see cref="OptionInterface.rwMod"/> is null.
    /// </summary>
    [Serializable]
    public class NullModException : NullReferenceException
    {
        public NullModException(string id) : base($"OptionInterface.rwMod is null! (id: {id})")
        {
        }

        public NullModException()
        {
        }

        public NullModException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NullModException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// OptionInterface hasn't enabled <see cref="OptionInterface.hasProgData"/>.
    /// </summary>
    public class NoProgDataException : InvalidOperationException
    {
        public NoProgDataException(OptionInterface oi) : base($"OptionInterface {oi.rwMod.ModID} hasn't enabled hasProgData")
        {
        }
    }

    /// <summary>
    /// OptionInterface tried to use an invalid Slugcat number.
    /// </summary>
    public class InvalidSlugcatException : ArgumentException
    {
        public InvalidSlugcatException(OptionInterface oi) : base($"OptionInterface {oi.rwMod.ModID} tried to use an invalid Slugcat number")
        {
        }
    }
}