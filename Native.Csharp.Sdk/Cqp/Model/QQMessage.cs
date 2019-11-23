﻿using Native.Csharp.Sdk.Cqp.Expand;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.Sdk.Cqp.Model
{
	/// <summary>
	/// 描述 QQ消息 的类
	/// </summary>
	public class QQMessage
	{
		#region --属性--
		/// <summary>
		/// 获取当前实例用于获取信息的 <see cref="Native.Csharp.Sdk.Cqp.CQApi"/> 实例对象
		/// </summary>
		public CQApi CQApi { get; private set; }

		/// <summary>
		/// 获取当前实例的消息ID
		/// </summary>
		public int Id { get; private set; }

		/// <summary>
		/// 获取当前实例的原始消息
		/// </summary>
		public string OriginalMessage { get; private set; }

		/// <summary>
		/// 获取一个值, 指示当前消息是否为正则消息
		/// </summary>
		public bool IsRegexMessage { get; private set; }

		/// <summary>
		/// 获取当前实例解析出的正则消息键值对
		/// </summary>
		public Dictionary<string, string> PairsMessage { get; private set; }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 初始化 <see cref="QQMessage"/> 类的新实例
		/// </summary>
		/// <param name="api">用于获取信息的实例</param>
		/// <param name="id">消息ID</param>
		/// <param name="msg">消息内容</param>
		public QQMessage (CQApi api, int id, string msg)
			: this (api, id, msg, false)
		{

		}
		/// <summary>
		/// 初始化 <see cref="QQMessage"/> 类的新实例
		/// </summary>
		/// <param name="api">用于获取信息的实例</param>
		/// <param name="id">消息ID</param>
		/// <param name="msg">消息内容</param>
		/// <param name="isRegex">是否正则</param>
		public QQMessage (CQApi api, int id, string msg, bool isRegex)
		{
			if (api == null)
			{
				throw new ArgumentNullException ("api");
			}

			if (id < 10000)
			{
				throw new ArgumentOutOfRangeException ("id");
			}

			if (msg == null)
			{
				throw new ArgumentNullException ("msg");
			}

			this.CQApi = api;
			this.Id = id;
			this.OriginalMessage = msg;
			this.IsRegexMessage = isRegex;
			this.PairsMessage = null;

			if (isRegex)
			{
				// 进行正则事件消息解析
				using (BinaryReader reader = new BinaryReader (new MemoryStream (Convert.FromBase64String (msg))))
				{
					if (reader.Length () < 4)
					{
						throw new InvalidDataException ("读取失败, 原始数据格式错误");
					}

					this.PairsMessage = new Dictionary<string, string> (reader.ReadInt32_Ex ());

					for (int i = 0; i < PairsMessage.Count; i++)
					{
						using (BinaryReader tempReader = new BinaryReader (new MemoryStream (reader.ReadToken_Ex ())))
						{
							if (tempReader.Length () < 4)
							{
								throw new InvalidDataException ("读取失败, 原始数据格式错误");
							}

							// 读取结果
							string key = tempReader.ReadString_Ex ();
							string content = tempReader.ReadString_Ex ();

							this.PairsMessage.Add (key, content);
						}
					}
				}
			}
		}
		#endregion

		#region --公开方法--
		/// <summary>
		/// 撤回消息
		/// </summary>
		/// <returns>撤回成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool RemoveMessage ()
		{
			return this.CQApi.RemoveMessage (this.Id);
		}

		/// <summary>
		/// 返回表示当前对象的字符串
		/// </summary>
		/// <returns>表示当前对象的字符串</returns>
		public override string ToString ()
		{
			StringBuilder builder = new StringBuilder ();
			builder.AppendLine (string.Format ("ID: {0}", this.Id));
			builder.AppendLine (string.Format ("消息内容: {0}", this.OriginalMessage));
			return builder.ToString ();
		}
		#endregion
	}
}
