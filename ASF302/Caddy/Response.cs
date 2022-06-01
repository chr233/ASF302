namespace ASF302.Caddy
{
	internal sealed class CmdResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; }

		public CmdResponse(bool success, string message)
		{
			Success = success;
			Message = message;
		}

		public override string ToString()
		{
			return $"{Success} {Message}";
		}
	}
}
