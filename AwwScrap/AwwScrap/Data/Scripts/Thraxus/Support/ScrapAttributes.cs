namespace AwwScrap.Support
{
	public struct ScrapAttributes
	{
		public readonly float Mass;
		public readonly float Volume;

		public ScrapAttributes(float mass, float volume)
		{
			Mass = mass;
			Volume = volume;
		}

		public override string ToString()
		{
			return $"Mass: {Mass} - Volume: {Volume}";
		}
	}
}
