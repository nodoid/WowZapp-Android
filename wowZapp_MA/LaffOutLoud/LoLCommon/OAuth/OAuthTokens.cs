namespace OAuthLib
{
    /// <include file='OAuthTokens.xml' path='OAuthTokens/OAuthTokens/*'/>
#if !SILVERLIGHT
    [System.Serializable]
#endif
    public class OAuthTokens
    {
        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        /// <value>The access token.</value>
        public string AccessToken { internal get; set; }

        /// <summary>
        /// Gets or sets the access token secret.
        /// </summary>
        /// <value>The access token secret.</value>
        public string AccessTokenSecret { internal get; set; }

        /// <summary>
        /// Gets or sets the consumer key.
        /// </summary>
        /// <value>The consumer key.</value>
        public string ConsumerKey { internal get; set; }

        /// <summary>
        /// Gets or sets the consumer secret.
        /// </summary>
        /// <value>The consumer secret.</value>
        public string ConsumerSecret { internal get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance has consumer token values.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has consumer token; otherwise, <c>false</c>.
        /// </value>
        public bool HasConsumerToken
        {
            get
            {
                return !string.IsNullOrEmpty(this.ConsumerKey) && !string.IsNullOrEmpty(this.ConsumerSecret);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has access token values.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has access token; otherwise, <c>false</c>.
        /// </value>
        public bool HasAccessToken
        {
            get
            {
                return !string.IsNullOrEmpty(this.AccessToken) && !string.IsNullOrEmpty(this.AccessTokenSecret);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has values. This does not verify that the values are correct.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has values; otherwise, <c>false</c>.
        /// </value>
        public bool HasBothTokens
        {
            get
            {
                return this.HasAccessToken && this.HasConsumerToken;
            }
        }
    }
}
