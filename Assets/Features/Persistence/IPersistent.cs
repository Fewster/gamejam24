using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPersistent 
{
    void Load(PersistentModel model);
    void Save(PersistentModel model);
}
