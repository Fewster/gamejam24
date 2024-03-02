using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPersistent 
{
    void Load(PersistenceModel model);
    void Save(PersistenceModel model);
}
