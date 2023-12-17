public enum PlayerInputType { MOUSE, KEYBOARD };

public class PlayerInput
{
    private int mouseButton;
    private string key;

    public PlayerInputType Type { get; private set; }

    private PlayerInput(int mouseButton)
    {
        Type = PlayerInputType.MOUSE;
        this.mouseButton = mouseButton;
    }

    private PlayerInput(string key)
    {
        Type = PlayerInputType.KEYBOARD;
        this.key = key;
    }

    public static PlayerInput MouseInput(int mouseButton)
    {
        return new PlayerInput(mouseButton);
    }

    public static PlayerInput KeyInput(string key)
    {
        return new PlayerInput(key);
    }

    public bool GetDown()
    {
        if (Type == PlayerInputType.MOUSE)
        {
            return UnityEngine.Input.GetMouseButtonDown(mouseButton);
        }
        else if (Type == PlayerInputType.KEYBOARD)
        {
            return UnityEngine.Input.GetKeyDown(key);
        }
        return false;
    }

    public bool GetUp()
    {
        if (Type == PlayerInputType.MOUSE)
        {
            return UnityEngine.Input.GetMouseButtonUp(mouseButton);
        }
        else if (Type == PlayerInputType.KEYBOARD)
        {
            return UnityEngine.Input.GetKeyUp(key);
        }
        return false;
    }

    public bool Get()
    {
        if (Type == PlayerInputType.MOUSE)
        {
            return UnityEngine.Input.GetMouseButton(mouseButton);
        }
        else if (Type == PlayerInputType.KEYBOARD)
        {
            return UnityEngine.Input.GetKey(key);
        }
        return false;
    }
}
