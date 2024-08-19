using System.Collections.Generic;

namespace UCode.DeepCloning
{
    internal delegate object CloneObjectDelegate(object source, Dictionary<object, object> reusableClones, DeepCloningOptions options);
}
