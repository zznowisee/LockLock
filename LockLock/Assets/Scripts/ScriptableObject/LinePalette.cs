using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class LinePalette : ScriptableObject
{
    public Color defaultCol;
    public Color highlightCol;
    public Color beSelectCol;

    public Color dottedWaitingCol;
    public Color dottedUsingCol;
}
