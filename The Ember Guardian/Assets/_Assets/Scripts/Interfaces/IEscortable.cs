using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEscortable
{
    public Transform GetEscortTransform();
    public Transform GetEscortMinTransform();
    public Transform GetEscortMaxTransform();
}
