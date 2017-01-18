# OAuth_CSharp
A C# library that allows OAuth 1.0a URL signature generation. For more information on how it works see: https://codetolight.wordpress.com/2017/01/16/oauth-for-woocommerce-in-unity3d/

Usage:
From your unity script use, add as follows.

OAuth_CSharp oauth = new OAuth_CSharp(consumerKey, consumerSecret);
string requestURL = oauth.GenerateRequestURL(in_url, "GET");

Then use HTTP to send to server (such as UnityWebRequest, in the case of Unity3D) to send request to server.
