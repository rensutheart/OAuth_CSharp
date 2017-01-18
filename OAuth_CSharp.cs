using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

public class OAuth_CSharp
{
    private string oauthConsumerKey = null;
    private string oauthConsumerSecret = null;
    private string oauthToken = null;
    private string oauthTokenSecret = null;
    private string oauthNonce = null;
    private string oauthTimeStamp = null;

    private const string oauthMethod = "HMAC-SHA1";
    private const string oauthVersion = "1.0";

    private SortedDictionary<string, string> urlParamsDictionary;
    private string outputString = "";

    public OAuth_CSharp(string consumerKey, string consumerSecret)
    {
        oauthConsumerKey = consumerKey;
        oauthConsumerSecret = consumerSecret;
        oauthNonce = GetNonce();
        oauthTimeStamp = GetTimestamp();

        GenerateDictionaryNoToken();
    }

    public OAuth_CSharp(string consumerKey, string consumerSecret, string token, string tokenSecret)
    {
        oauthConsumerKey = consumerKey;
        oauthConsumerSecret = consumerSecret;
        oauthToken = token;
        oauthTokenSecret = tokenSecret;
        oauthNonce = GetNonce();
        oauthTimeStamp = GetTimestamp();

        GenerateDictionary();
    }

    public string GenerateRequestURL(string url, string HTTP_Method , List<string> parameters)
    {
        outputString = url;

        // outputString is changed in GenerateSignatureBase and GenerateSignature
        string signatureBase = GenerateSignatureBase(url, HTTP_Method, parameters);
        string signature = GenerateSignature(signatureBase);
        
        return outputString;
    }

    public string GenerateRequestURL(string url, string HTTP_Method)
    {
        outputString = url;

        // outputString is changed in GenerateSignatureBase and GenerateSignature
        string signatureBase = GenerateSignatureBase(url, HTTP_Method);
        string signature = GenerateSignature(signatureBase);

        return outputString;
    }


    private string GetNonce()
    {
        //string nonce = rand.Next(123400, 9999999).ToString();

        /* http://www.i-avington.com/Posts/Post/making-a-twitter-oauth-api-call-using-c
        System.Random rand = new System.Random();
        string nonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture)));
        return nonce;
        */

        string nonce = Guid.NewGuid().ToString("N");
        return nonce;        
    }

    private string GetTimestamp()
    {
        /*
        // http://www.i-avington.com/Posts/Post/making-a-twitter-oauth-api-call-using-c
        TimeSpan _timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
        string timeStamp = Convert.ToInt64(_timeSpan.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        return timeStamp;
        */

        
        //http://stackoverflow.com/questions/16919320/c-sharp-oauth-and-signatures
        TimeSpan ts = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0));
        string timeStamp = ts.TotalSeconds.ToString();
        timeStamp = timeStamp.Substring(0, timeStamp.IndexOf("."));
        return timeStamp;
    }

    private void GenerateDictionary()
    {
        //the key value pairs have to be sorted by encoded key
        urlParamsDictionary = new SortedDictionary<string, string>()
        {			
			{Uri.EscapeDataString("oauth_consumer_key"), Uri.EscapeDataString(oauthConsumerKey)},
            {Uri.EscapeDataString("oauth_nonce"), Uri.EscapeDataString(oauthNonce)},
            {Uri.EscapeDataString("oauth_signature_method"), Uri.EscapeDataString(oauthMethod)},
            {Uri.EscapeDataString("oauth_timestamp"), Uri.EscapeDataString(oauthTimeStamp)},
            {Uri.EscapeDataString("oauth_token"), Uri.EscapeDataString(oauthToken)},
            {Uri.EscapeDataString("oauth_version"), Uri.EscapeDataString(oauthVersion)}
        };
    }

    private void GenerateDictionaryNoToken()
    {
        //the key value pairs have to be sorted by encoded key
        urlParamsDictionary = new SortedDictionary<string, string>()
        {			
			{Uri.EscapeDataString("oauth_consumer_key"), Uri.EscapeDataString(oauthConsumerKey)},
            {Uri.EscapeDataString("oauth_nonce"), Uri.EscapeDataString(oauthNonce)},
            {Uri.EscapeDataString("oauth_signature_method"), Uri.EscapeDataString(oauthMethod)},
            {Uri.EscapeDataString("oauth_timestamp"), Uri.EscapeDataString(oauthTimeStamp)},           
            {Uri.EscapeDataString("oauth_version"), Uri.EscapeDataString(oauthVersion)}
        };
    }

    private string GenerateSignatureBase(string url, string HTTP_Method, List<string> parameters)
    {
        if(parameters == null)
        {
            Debug.Log("Invalid parameters supplied: null. Running without parameters.");
            return GenerateSignatureBase(url, HTTP_Method);            
        }
        if (parameters.Count != 0)
        {
            foreach (string parameter in parameters)
            {
                //Debug.Log(parameters);
                string[] elements = parameter.Split(new char[] { '=' });
                if (elements.Length % 2 != 0)
                {
                    Debug.LogWarning("Invalid parameter supplied");
                }
                else
                {
                    try
                    {
                        urlParamsDictionary.Add(elements[0], elements[1]);
                    }
                    catch(ArgumentException argExc)
                    {
                        Debug.LogWarning("Parameter key already in dictionary: " + elements[0]);
                    }
                }
            }
        }

        return GenerateSignatureBase(url, HTTP_Method);
    }

    private string GenerateSignatureBase(string url, string HTTP_Method)
    { 
        string parameterString = String.Empty;
        
        int totalKeys = urlParamsDictionary.Keys.Count;
        int keyCount = 0;
        foreach (KeyValuePair<string, string> keyValuePair in urlParamsDictionary)
        {
           // Debug.Log("Appended to output: " + keyValuePair.Key + "=" + keyValuePair.Value);
            parameterString += keyValuePair.Key + "=" + keyValuePair.Value;

            if (++keyCount != totalKeys) // indicates the last item is found
                parameterString += "&";
        }
        
        outputString += "?" + parameterString;
        //Debug.Log(parameterString);

        string signatureBase = HTTP_Method.ToUpper() + "&" + Uri.EscapeDataString(url) + "&" + Uri.EscapeDataString(parameterString);
        
        Debug.Log("Signature Base: " + signatureBase); ;
        return signatureBase;
    }

    private string GenerateSignature(string signatureBase)
    {
        string signatureKey = "";
        if(oauthToken != null && oauthTokenSecret != null)
            signatureKey = Uri.EscapeDataString(oauthConsumerSecret) + "&" + Uri.EscapeDataString(oauthTokenSecret);
        else
            signatureKey = Uri.EscapeDataString(oauthConsumerSecret) + "&";
        HMACSHA1 hmacsha1 = new HMACSHA1(new ASCIIEncoding().GetBytes(signatureKey));

        //hash the values
        string signature = Convert.ToBase64String(hmacsha1.ComputeHash(new ASCIIEncoding().GetBytes(signatureBase)));

        outputString += "&oauth_signature=" + signature;

        return signature;
    }
}
