using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class ParallelPort : ConfigurableComponent {
    private bool parallelflip = false;
    public int portHexAddress;

    [DllImport("inpoutx64.dll", EntryPoint = "Out32")]
    private static extern void Out32(int address, int value);

    [Serializable]
    public class Settings : ComponentSettings {
        public int portHexAddress;
        public Settings(int portHexAddress) {
            this.portHexAddress = portHexAddress;
        }
    }

    public static void TryOut32(int address, int value) {
        try {
            Out32(address, value);
        }
        catch (System.DllNotFoundException e) {
            Debug.LogException(e);
        }
    }

    public void TryWriteTrigger(SessionTrigger trigger, int rewardIndex) {
        
	// kw edit (debug)
	Debug.Log("tryingtrigger");
	Debug.Log($"Port \"{portHexAddress}\" being used");  
	Debug.Log($"Value \"{(int)trigger + rewardIndex}\" being used");
	
	if (portHexAddress != -1) { // kw edit (flipped to !=)
	    Debug.Log("entered if statement");
            Out32(portHexAddress, (int)trigger + rewardIndex);
	    Out32(portHexAddress, 0);
        }
	
    }

    public void SimpleTest() {
        if (parallelflip) {
            TryOut32(portHexAddress, 222);
        }
        else {
            TryOut32(portHexAddress, 0);
        }

        parallelflip = !parallelflip;
    }

    public override Type GetSettingsType() {
        return typeof(Settings);
    }

    public override ComponentSettings GetDefaultSettings() {
        return new Settings(0);
    }

    public override ComponentSettings GetCurrentSettings() {
        return new Settings(portHexAddress);
    }

    protected override void ApplySettings(ComponentSettings loadedSettings) {
        Settings s = (Settings)loadedSettings;

        portHexAddress = s.portHexAddress;
    }
}
