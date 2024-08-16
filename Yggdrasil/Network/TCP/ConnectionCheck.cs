namespace Yggdrasil.Network.TCP
{
	/// <summary>
	/// Result of a connection validity check.
	/// </summary>
	public struct ConnectionCheck
	{
		/// <summary>
		/// Specifies whether to accept or reject the connection.
		/// </summary>
		public readonly ConnectionCheckResult Result;

		/// <summary>
		/// Specifies the reason for the decision.
		/// </summary>
		public readonly string Reason;

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="reason"></param>
		public ConnectionCheck(ConnectionCheckResult type, string reason)
		{
			this.Result = type;
			this.Reason = reason;
		}

		/// <summary>
		/// Returns a string representation of this object.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return $"{this.Result}: {this.Reason}";
		}
	}

	/// <summary>
	/// Specifies the type of connection check result.
	/// </summary>
	public enum ConnectionCheckResult
	{
		/// <summary>
		/// Accept the connection.
		/// </summary>
		Accept,

		/// <summary>
		/// Reject the connection.
		/// </summary>
		Reject,
	}
}
