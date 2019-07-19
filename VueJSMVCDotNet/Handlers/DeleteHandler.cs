﻿using Org.Reddragonit.VueJSMVCDotNet.Attributes;
using Org.Reddragonit.VueJSMVCDotNet.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Org.Reddragonit.VueJSMVCDotNet.Handlers
{
    internal class DeleteHandler : IRequestHandler
    {
        private Dictionary<string, MethodInfo> _loadMethods;
        private Dictionary<string, MethodInfo> _deleteMethods;

        public DeleteHandler()
        {
            _loadMethods = new Dictionary<string, MethodInfo>();
            _deleteMethods = new Dictionary<string, MethodInfo>();
        }

        public void ClearCache()
        {
            _loadMethods.Clear();
            _deleteMethods.Clear();
        }

        public string HandleRequest(string url, RequestHandler.RequestMethods method, string formData, out string contentType, out int responseStatus)
        {
            IModel model = null;
            lock (_loadMethods)
            {
                if (_loadMethods.ContainsKey(url.Substring(0, url.LastIndexOf("/"))))
                    model = (IModel)_loadMethods[url.Substring(0, url.LastIndexOf("/"))].Invoke(null, new object[] { url.Substring(url.LastIndexOf("/") + 1) });
            }
            if (model == null)
            {
                contentType = "text/text";
                responseStatus = 404;
                return "Model Not Found";
            }
            else
            {
                MethodInfo mi = null;
                lock (_deleteMethods)
                {
                    if (_deleteMethods.ContainsKey(url.Substring(0, url.LastIndexOf("/"))))
                        mi = _deleteMethods[url.Substring(0, url.LastIndexOf("/"))];
                }
                if (mi != null)
                {
                    contentType = "text/json";
                    responseStatus = 200;
                    return JSON.JsonEncode(mi.Invoke(model, new object[] { }));
                }
                else
                {
                    contentType = "text/text";
                    responseStatus = 404;
                    return "Not Found";
                }
            }
        }

        public bool HandlesRequest(string url, RequestHandler.RequestMethods method)
        {
            if (method == RequestHandler.RequestMethods.DELETE)
                return _deleteMethods.ContainsKey(url.Substring(0, url.LastIndexOf("/")))&& _loadMethods.ContainsKey(url.Substring(0, url.LastIndexOf("/")));
            return false;
        }

        public void Init(List<Type> types)
        {
            lock (_deleteMethods)
            {
                _loadMethods.Clear();
                _deleteMethods.Clear();
                foreach (Type t in types)
                {
                    foreach (MethodInfo mi in t.GetMethods(Constants.STORE_DATA_METHOD_FLAGS))
                    {
                        if (mi.GetCustomAttributes(typeof(ModelDeleteMethod), false).Length > 0)
                        {
                            _deleteMethods.Add(Utility.GetModelUrlRoot(t), mi);
                            foreach (MethodInfo m in t.GetMethods(Constants.LOAD_METHOD_FLAGS))
                            {
                                if (m.GetCustomAttributes(typeof(ModelLoadMethod), false).Length > 0)
                                {
                                    _loadMethods.Add(Utility.GetModelUrlRoot(t), m);
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }
    }
}