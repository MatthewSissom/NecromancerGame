[System.Serializable]
public class PID
{
	public float pFactor, iFactor, dFactor;

	float integral;
	float lastError;

	public PID(float pFactor, float iFactor, float dFactor)
	{
		this.pFactor = pFactor;
		this.iFactor = iFactor;
		this.dFactor = dFactor;
		Reset();
	}

	public void Reset()
    {
		integral = 0;
		lastError = 0;
    }

	public float Update(float present, float timeFrame)
	{
		//0 out intergral when error crosses or touches 0
		if (present * lastError <= 0)
			integral = 0;
		integral += present * timeFrame;
		float deriv = (present - lastError) / timeFrame;
		lastError = present;
		return present * pFactor + integral * iFactor + deriv * dFactor;
	}
}
