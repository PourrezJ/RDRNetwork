using System;

namespace ResuMPServer.Exceptions
{
    public class EntityDeletedException : Exception
    {
        internal EntityDeletedException(Entity entity) : base($"This entity ({entity.Type.ToString()}: ID {entity.Id}) does not exist anymore!")
        {

        }

        internal EntityDeletedException(Entity entity, string parameterName) : base($"Parameter {parameterName}: This entity ({entity.Type.ToString()}: ID {entity.Id}) does not exist anymore!")
        {

        }
    }
}
