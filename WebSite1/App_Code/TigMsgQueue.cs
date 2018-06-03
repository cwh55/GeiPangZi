using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Messaging;
/// <summary>
/// TigMsgQueue 的摘要说明
/// </summary>
public class TigMsgQueue<T> where T : class 
{
    /// <summary>
    /// 私有MSMQ队列
    /// </summary>
    /// <param name="name">队列名</param>
    /// <param name="targetType">要在队列中存储的消息类型</param>
    public TigMsgQueue(string name)
    {
        this.QueueName = name;
        this.Formater = new XmlMessageFormatter(new Type[] { typeof(T) });
    }

    /// <summary> 格式类似 @".\Private$\MSMQDemo" </summary>
    public string QueueName { get; private set; }

    public XmlMessageFormatter Formater { get; private set; }

    private MessageQueue _msgQueue = null;
    public MessageQueue MsgQueue
    {
        get
        {
            if (_msgQueue == null)
            {
                if (MessageQueue.Exists(QueueName))
                {
                    _msgQueue = new MessageQueue(QueueName);
                }
                else
                {
                    // 此处不建议使用程序创建队列，那样可能导致消息队列访问权限的问题，如使用Local Service帐号创建时，其他帐号都没有权限修改
                    _msgQueue = MessageQueue.Create(QueueName);

                    //throw new ApplicationException("不存在名为" + QueueName + "的消息队列");
                }
            }

            return _msgQueue;
        }
    }

    public void Send(T value)
    {
        var msg = new Message(value, Formater);
        MsgQueue.Send(msg);
    }

    /// <summary>
    /// 接收一条消息，并从队列中删除
    /// </summary>
    /// <returns>如果没有消息则返回null</returns>
    public T Receive()
    {
        // 1秒
        TimeSpan timeoutSpan = new TimeSpan(0, 0, 1);

        Message msg = null;

        try
        {
            msg = MsgQueue.Receive(timeoutSpan);
            msg.Formatter = this.Formater;
            return msg.Body as T;
        }
        catch (MessageQueueException ex)
        {
            if (ex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
            {
                // 超时，说明还没有消息
                return null;
            }
            else
            {
                throw ex;
            }
        }
    }
}