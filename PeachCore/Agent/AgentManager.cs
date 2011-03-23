﻿
//
// Copyright (c) Michael Eddington
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in	
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

// Authors:
//   Michael Eddington (mike@phed.org)

// $Id$

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Peach.Core.Dom;

namespace Peach.Core.Agent
{
	/// <summary>
	/// Manages all agents.  This includes
	/// full lifetime.
	/// </summary>
	public class AgentManager
	{
		static int UniqueNames = 0;
		OrderedDictionary<string, AgentServer> _agents = new OrderedDictionary<string, AgentServer>();
		Dictionary<string, Dom.Agent> _agentDefinitions = new Dictionary<string, Dom.Agent>();

		public AgentManager()
		{
		}

		public virtual void AddAgent(Dom.Agent agentDef)
		{
			Uri uri = new Uri(agentDef.url);
			Type tAgent = GetAgentByProtocol(uri);
			if (tAgent == null)
				throw new PeachException("Error, unable to locate agent that supports the '" + uri.Scheme + "' protocol.");

			ConstructorInfo co = tAgent.GetConstructor(new Type[] { typeof(string), typeof(string), typeof(string) });
			AgentServer agent = (AgentServer)co.Invoke(new object[] { agentDef.name, agentDef.url, agentDef.password });

			_agents[agentDef.name] = agent;
			_agentDefinitions[agentDef.name] = agentDef;
		}

		public virtual void AgentConnect(string name)
		{
			Dom.Agent def = _agentDefinitions[name];
			AgentServer agent = _agents[name];

			agent.AgentConnect(def.name, def.url, def.password);

			foreach (Dom.Monitor mon in def.monitors)
			{
				agent.StartMonitor("Monitor_" + UniqueNames, mon.cls, mon.parameters);
				UniqueNames++;
			}
		}

		public virtual void AgentConnect(Dom.Agent agent)
		{
			if (!_agents.Keys.Contains(agent.name))
				AddAgent(agent);

			AgentConnect(agent.name);
		}

		public Type GetAgentByProtocol(Uri uri)
		{
			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type t in a.GetExportedTypes())
				{
					if (!t.IsClass)
						continue;

					foreach (object attrib in t.GetCustomAttributes(true))
					{
						if (attrib is AgentAttribute && ((AgentAttribute)attrib).protocol == uri.Scheme)
						{
							return t;
						}
					}
				}
			}

			return null;
		}

		#region AgentServer

		public virtual void StopAllMonitors()
		{
			foreach (AgentServer agent in _agents.Values)
				agent.StopAllMonitors();
		}

		public virtual void SessionStarting()
		{
			foreach (AgentServer agent in _agents.Values)
				agent.SessionStarting();
		}

		public virtual void SessionFinished()
		{
			foreach (AgentServer agent in _agents.Values)
				agent.SessionFinished();
		}

		public virtual void IterationStarting(int iterationCount, bool isReproduction)
		{
			foreach (AgentServer agent in _agents.Values)
				agent.IterationStarting(iterationCount, isReproduction);
		}

		public virtual bool IterationFinished()
		{
			bool ret = false;

			foreach (AgentServer agent in _agents.Values)
				if (agent.IterationFinished())
					ret = true;

			return ret;
		}

		public virtual bool DetectedFault()
		{
			bool ret = false;

			foreach (AgentServer agent in _agents.Values)
				if (agent.DetectedFault())
					ret = true;

			return ret;
		}

		public virtual Dictionary<AgentServer, System.Collections.Hashtable> GetMonitorData()
		{
			Dictionary<AgentServer, System.Collections.Hashtable> data = new Dictionary<AgentServer, System.Collections.Hashtable>();

			foreach (AgentServer agent in _agents.Values)
				data[agent] = agent.GetMonitorData();

			return data;
		}

		public virtual bool MustStop()
		{
			bool ret = false;
			foreach (AgentServer agent in _agents.Values)
				if (agent.MustStop())
					ret = true;

			return ret;
		}

		#endregion
	}
}
