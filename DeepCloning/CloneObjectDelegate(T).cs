using System.Collections.Generic;

namespace UCode.DeepCloning
{
    internal delegate T CloneObjectDelegate<T>(T source, Dictionary<object, object> reusableClones, DeepCloningOptions options);
}
