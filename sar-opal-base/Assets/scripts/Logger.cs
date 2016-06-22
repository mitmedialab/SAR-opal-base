using UnityEngine;
using System;

namespace opal
{
    /// <summary>
    /// The Logger class wraps Unity's Debug.Log* calls, with the main goal of
    /// adding additional information (such as a timestamp) to each log message.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Log a general message.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="context">Context.</param>
        public static void Log(object obj, UnityEngine.Object context = null)
        {
            Debug.Log(String.Concat(System.DateTime.UtcNow.ToString(
                "[yyyy-MM-dd HH:mm:ss.fff] "), obj), context);
        }

        /// <summary>
        /// Log a warning.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="context">Context.</param>
        public static void LogWarning(object obj, UnityEngine.Object context = null)
        {    
            Debug.LogWarning(String.Concat(System.DateTime.UtcNow.ToString(
                "[yyyy-MM-dd HH:mm:ss.fff] "), obj), context);
        }

        /// <summary>
        /// Log an error.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="context">Context.</param>
        public static void LogError(object obj, UnityEngine.Object context = null)
        {
            Debug.LogError(String.Concat(System.DateTime.UtcNow.ToString(
                "[yyyy-MM-dd HH:mm:ss.fff] "), obj), context);
        }

    }
}

