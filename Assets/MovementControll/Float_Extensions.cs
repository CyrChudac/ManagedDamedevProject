using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Float_Extensions
{
	public static int Sign(this float x)
		=> x > 0 ? 1 : (x < 0 ? -1 : 0);
}
