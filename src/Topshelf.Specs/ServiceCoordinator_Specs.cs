// Copyright 2007-2008 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Specs
{
	using System;
	using System.Collections.Generic;
	using Configuration;
	using Internal;
	using MbUnit.Framework;
	using Microsoft.Practices.ServiceLocation;
	using Rhino.Mocks;

	[TestFixture]
	public class ServiceCoordinator_Specs
	{
		private TestService _service;
		private ServiceCoordinator _serviceCoordinator;

		[SetUp]
		public void EstablishContext()
		{
			_service = new TestService();
			IServiceLocator sl = MockRepository.GenerateMock<IServiceLocator>();
			ServiceLocator.SetLocatorProvider(() => sl);

			sl.Stub(x => x.GetInstance<TestService>("test")).Return(_service).Repeat.Any();

			_serviceCoordinator = new ServiceCoordinator(x => { }, x => { }, x => { });
			IList<Func<IService>> services = new List<Func<IService>>
				{
					() => 
					new Service<TestService>
						{
							CreateServiceLocator = () => sl,
							Name = "test",
							StartAction = x => x.Start(),
							StopAction = x => x.Stop(),
							ContinueAction = x => x.Continue(),
							PauseAction = x => x.Pause()
						}
				};

			_serviceCoordinator.RegisterServices(services);
		}

		[Test]
		public void Unstarted_coordinator_should_not_start_services()
		{
			_service.HasBeenStarted
				.ShouldBeFalse();
		}

		[Test]
		public void Starting_the_coordinator_should_start_services()
		{
			_serviceCoordinator.Start();
			_service.HasBeenStarted
				.ShouldBeTrue();
		}

		[Test]
		public void Stopping_the_coordinator_should_stop_services()
		{
			_serviceCoordinator.Start();
			_serviceCoordinator.Stop();

			_service.HasBeenStarted
				.ShouldBeTrue();
			_service.Stopped
				.ShouldBeTrue();
		}

		[Test]
		public void Pausing_the_coordinator_should_pause_services()
		{
			_serviceCoordinator.Start();
			_serviceCoordinator.Pause();

			_service.HasBeenStarted
				.ShouldBeTrue();
			_service.Paused
				.ShouldBeTrue();
		}

		[Test]
		public void Continueing_the_coordintaro_should_continue_services()
		{
			_serviceCoordinator.Start();
			_serviceCoordinator.Pause();
			_serviceCoordinator.Continue();

			_service.HasBeenStarted
				.ShouldBeTrue();
			_service.HasBeenContinued
				.ShouldBeTrue();
			_service.Started
				.ShouldBeTrue();
		}

		[Test, Ignore]
		public void You_cant_pause_if_you_havent_started()
		{
			Assert.Fail();
		}

		[Test, Ignore]
		public void You_cant_continue_if_you_arent_currently_paused()
		{
			Assert.Fail();
		}
	}

	[TestFixture]
	public class SingleServiceActions
	{
		private TestService _service;
		private TestService2 _service2;
		private ServiceCoordinator _serviceCoordinator;

		[SetUp]
		public void EstablishContext()
		{
			_service = new TestService();
			_service2 = new TestService2();
			IServiceLocator sl = MockRepository.GenerateMock<IServiceLocator>();
			ServiceLocator.SetLocatorProvider(() => sl);

			sl.Stub(x => x.GetInstance<TestService>("test")).Return(_service).Repeat.Any();
			sl.Stub(x => x.GetInstance<TestService2>("test2")).Return(_service2).Repeat.Any();

			_serviceCoordinator = new ServiceCoordinator(x => { }, x => { }, x => { });
			IList<Func<IService>> services = new List<Func<IService>>
				{
					()=>
					new Service<TestService>
						{
							CreateServiceLocator = () => sl,
							Name = "test",
							StartAction = x => x.Start(),
							StopAction = x => x.Stop(),
							ContinueAction = x => x.Continue(),
							PauseAction = x => x.Pause()
						},
						()=>
					new Service<TestService2>
						{
							CreateServiceLocator = () => sl,
							Name = "test2",
							StartAction = x => x.Start(),
							StopAction = x => x.Stop(),
							ContinueAction = x => x.Continue(),
							PauseAction = x => x.Pause()
						}
				};

			_serviceCoordinator.RegisterServices(services);
			_serviceCoordinator.Start();
		}

		[Test]
		public void Stop_individual_service()
		{
			_serviceCoordinator.StopService("test");

			_service.Stopped
				.ShouldBeTrue();
			_service2.Started
				.ShouldBeTrue();
		}

		[Test]
		public void Start_individual_service()
		{
			_serviceCoordinator.Pause();
			_serviceCoordinator.StartService("test");

			_service.Started
				.ShouldBeTrue();
			_service2.Paused
				.ShouldBeTrue();
		}

		[Test]
		public void Pause_individual_service()
		{
			_serviceCoordinator.PauseService("test");

			_service.Paused
				.ShouldBeTrue();
			_service2.Started
				.ShouldBeTrue();
		}

		[Test]
		public void Continue_individual_service()
		{
			_serviceCoordinator.PauseService("test");
			_serviceCoordinator.ContinueService("test");

			_service.Started
				.ShouldBeTrue();
			_service.HasBeenContinued
				.ShouldBeTrue();
			_service2.Started
				.ShouldBeTrue();
		}
	}
}