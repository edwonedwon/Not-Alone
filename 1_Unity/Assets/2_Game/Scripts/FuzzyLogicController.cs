using System.Collections;




public class FuzzyLogicController
{
	public ArrayList RuleList = new ArrayList();
	
	public FuzzyLogicController()
	{
		
	}
	
	public float DeFuzz()
	{
		float vals = 0.0f;
		float weights = 0.0f;
		
		foreach(FuzzyRule flc in RuleList)
		{
			flc.Fuzzify();
			float area = flc.Area();
			weights += area;
			vals += flc.ComputeCentorid() * area;
		}
		return vals / weights;
	}
};


public class FuzzyRule
{
	public float x0;
	public float x1;
	
	public float x2;
	public float x3;
	
	
	private float membershipAmount;
	private float inputValue;
	
	public FuzzyRule(float _x0, float _x1, float _x2, float _x3, float input)
	{
		x0 = _x0;
		x1 = _x1;
		x2 = _x2;
		x3 = _x3;
		
		inputValue = input;		
		Fuzzify();
	}	
	
	public float Fuzzify()
	{	
		if (inputValue > x0 && inputValue < x1)
			membershipAmount = (x1 - inputValue) / (x1 - x0);
		else if (inputValue >= x1 && inputValue <= x2)
			membershipAmount = 1;
		else if (inputValue > x2 && inputValue <= x3)
			membershipAmount = (inputValue - x2) / (x3 - x2);
		else
			membershipAmount = 0;
		return membershipAmount;
	}
	
	public float ComputeCentorid()
    {
		float a = x2 - x1;
		float b = x3 - x0;
		float c = x1 - x0;		
		return ((2 * a * c) + (a * a) + (c * b) + (a * b) + (b * b)) / (3 * (a + b)) + x0; 
    }
	
	public float Area()
	{
		float b1 = x1-x0;
		float b2 = x3-x2;
		float height = x3 - x0;
		return ((b1+b2)*0.5f) / height;
	}
}
