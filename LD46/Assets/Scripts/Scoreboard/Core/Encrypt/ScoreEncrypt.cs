using ScoreboardCore.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScoreboardCore.Encrypt
{
    public static class ScoreEncrypt
    {
        public static ScoreSubmissionRequest CreateRequestForScore(Score _score, string _key)
        {
            ScoreSubmissionRequest request = new ScoreSubmissionRequest();
            request.Score = _score;
            request.Key = GetEncryptedKey(_score, _key);
            return request;
        }

        public static bool ValidateScoreRequest(ScoreSubmissionRequest _request, string _key)
        {
            string correctKey = GetEncryptedKey(_request.Score, _key);
            return (correctKey == _request.Key);
        }

        private static string GetEncryptedKey(Score _score, string _key)
        {
            string stringToEncrypt = _score.User + _score.Level + _score.ScoreValue;
            return Encrypt(stringToEncrypt, _key, 1);
        }


        // Stolen from rosetta code
        private static string Encrypt(string txt, string pw, int d)
        {
            // TODO NUMBERS LMAO

            int pwi = 0, tmp;
            string ns = "";
            txt = txt.ToUpper();
            pw = pw.ToUpper();

            int lowerBound = 48;
            int upperBound = 90;
            int range = upperBound - lowerBound;

            foreach (char t in txt)
            {
                if (t < lowerBound) continue;
                tmp = t - lowerBound + d * (pw[pwi] - lowerBound);
                if (tmp < 0) tmp += range;
                ns += Convert.ToChar(lowerBound + (tmp % range));
                if (++pwi == pw.Length) pwi = 0;
            }

            return ns;
        }
    }
}
