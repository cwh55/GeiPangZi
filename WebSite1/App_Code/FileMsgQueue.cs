using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// FileMsgQueue 的摘要说明
/// </summary>
public class FileMsgQueue<T> : TigMsgQueue<T> where T : class
{
    public FileMsgQueue()
        : base(@".\private$\jiexibaiduurl")
        { }
    
}