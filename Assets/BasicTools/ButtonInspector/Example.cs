using UnityEngine;
using BasicTools.ButtonInspector;

public class Example : MonoBehaviour {

	[Button("Button: Hello World", "debug")]
	public bool button_1;

	[Button("Button: Undo register", "change", true)]
	public bool button_2;

	public string text;

	public void debug() {
		Debug.Log("Hello World!");
	}

	public void change() {
		text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[Random.Range(0, 26)].ToString();
	}
}
