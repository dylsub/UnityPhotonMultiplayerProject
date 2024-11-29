using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyPanelBase : MonoBehaviour
{
    public enum LobbyPanelType {
        None,
        CreateNicknamePanel,
        MiddleSectionPanel,
    }

    [SerializeField] private LobbyPanelType PanelType;
    [SerializeField] private Animator panelAnimator;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
