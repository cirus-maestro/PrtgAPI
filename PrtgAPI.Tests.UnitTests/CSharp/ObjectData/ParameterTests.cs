﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Attributes;
using PrtgAPI.Parameters;
using PrtgAPI.Utilities;
using PrtgAPI.Tests.UnitTests.Infrastructure;
using PrtgAPI.Tests.UnitTests.Support;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectData
{
    class FakeSensorParameters : RawSensorParameters
    {
        public FakeSensorParameters() : base("fake_name", "fake_type")
        {
        }

        public int RestartStage
        {
            get { return (int)GetCustomParameterEnumXml<int>(ObjectProperty.AutoDiscoverySchedule); }
            set { SetCustomParameterEnumXml(ObjectProperty.AutoDiscoverySchedule, value); }
        }
    }

    [TestClass]
    public class ParameterTests : BaseTest
    {
        #region TableParameters

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TableParameters_CanSetSortDirection()
        {
            var parameters = new SensorParameters();

            parameters.SortBy = Property.Id;
            Assert.AreEqual(parameters.SortBy, Property.Id, "Retrieve initial value from property");
            Assert.AreEqual(parameters[Parameter.SortBy], Property.Id, "Retrieve initial raw value from indexer");

            parameters.SortDirection = SortDirection.Descending;
            Assert.AreEqual(parameters.SortBy, Property.Id, "Retrieve initial reversed value from property");
            Assert.AreEqual(parameters[Parameter.SortBy], "-objid", "Retrieve initial reversed raw value from indexer");

            parameters.SortBy = Property.Name;
            Assert.AreEqual(parameters.SortBy, Property.Name, "Retrieve new value from property");
            Assert.AreEqual(parameters[Parameter.SortBy], "-name", "Retrieve new reversed raw value from indexer");

            parameters.SortDirection = SortDirection.Ascending;
            Assert.AreEqual(parameters.SortBy, Property.Name, "Retrieve new forwards value from property");
            Assert.AreEqual(parameters[Parameter.SortBy], Property.Name, "Retrieve new forwards raw value from indexer");

            parameters.SortBy = null;
            Assert.AreEqual(parameters.SortBy, null, "Property value is null");
            Assert.AreEqual(parameters[Parameter.SortBy], null, "Raw value is null");

            parameters.SortDirection = SortDirection.Descending;
            parameters.SortBy = Property.Active;

            parameters.SortBy = null;
            Assert.AreEqual(parameters.SortBy, null, "Reversed property value is null");
            Assert.AreEqual(parameters[Parameter.SortBy], null, "Reversed raw value is null");

            parameters[Parameter.SortBy] = "name";
            Assert.AreEqual(parameters.SortBy, Property.Name, "Retrieve property directly set using string");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TableParameters_AddsAndRemovesFilters()
        {
            var parameters = new SensorParameters();
            Assert.AreEqual(false, parameters.RemoveFilters(new SearchFilter(Property.Comments, "hello")));
            var filter1 = new SearchFilter(Property.Id, 1234);

            parameters.AddFilters(filter1);
            Assert.AreEqual(1, parameters.SearchFilters.Count);

            var filters2_3 = new[] {new SearchFilter(Property.Id, 4567), new SearchFilter(Property.Type, "ping")};
            parameters.AddFilters(filters2_3);
            Assert.AreEqual(3, parameters.SearchFilters.Count);

            parameters.RemoveFilters(filters2_3);
            Assert.AreEqual(1, parameters.SearchFilters.Count);
            Assert.AreEqual(filter1, parameters.SearchFilters.Single());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TableParameters_AddRemoveFilters_ThrowsSpecifyingNull()
        {
            var parameters = new SensorParameters();

            AssertEx.Throws<ArgumentNullException>(() => parameters.AddFilters(null), "Value cannot be null.\r\nParameter name: filters");
            AssertEx.Throws<ArgumentNullException>(() => parameters.RemoveFilters(null), "Value cannot be null.\r\nParameter name: filters");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TableParameters_AddFilters_ToSearchFiltersProperty()
        {
            var parameters = new SensorParameters();
            Assert.IsNotNull(parameters.SearchFilters);
            Assert.AreEqual(0, parameters.SearchFilters.Count);

            var list = parameters.SearchFilters;
            list.Add(new SearchFilter(Property.Name, "name"));
            Assert.AreEqual(1, parameters.SearchFilters.Count);

            parameters.SearchFilters = null;
            Assert.IsNotNull(parameters.SearchFilters);
            Assert.AreEqual(0, parameters.SearchFilters.Count);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TableParameters_MergesMultiParameterFilterValues_SameProperty()
        {
            var sensor = new SensorParameters
            {
                Status = new[] {Status.Up, Status.Paused}
            };

            sensor.SearchFilters.Add(new SearchFilter(Property.Status, new[] {Status.Down, Status.Warning}));

            var statuses = sensor.Status;

            AssertEx.AreEqualLists(
                statuses.ToList(),
                new List<Status> {Status.Up, Status.Paused, Status.Down, Status.Warning},
                "Lists were not equal"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TableParameters_MergesMultiParameterFilterValues_IgnoresDifferentProperty()
        {
            var sensor = new SensorParameters
            {
                Status = new[] { Status.Up, Status.Paused }
            };

            sensor.SearchFilters.Add(new SearchFilter(Property.Name, "blah"));
            Assert.AreEqual(3, sensor.SearchFilters.Count);

            AssertEx.AreEqualLists(
                sensor.Status.ToList(),
                new List<Status> { Status.Up, Status.Paused },
                "Lists were not equal"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TableParameters_MergesMultipleParameterFilterValues_AddsInvalidValue()
        {
            var sensor = new SensorParameters
            {
                Status = new[] { Status.Up, Status.Paused },
                SearchFilters = { new SearchFilter(Property.Status, "blah") }
            };

            AssertEx.AreEqualLists(
                sensor.Status.ToList(),
                new List<Status> { Status.Up, Status.Paused },
                "Lists were not equal"
            );
        }

        #endregion
        #region CustomParameters

        [TestMethod]
        [TestCategory("UnitTest")]
        public void CustomParameter_ToString_FormatsCorrectly()
        {
            var parameter = new CustomParameter("name", "val");

            Assert.AreEqual("name=val", parameter.ToString());
        }

        #endregion
        #region SensorParameters

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SensorParameters_Status_CanBeGetAndSet()
        {
            var parameters = new SensorParameters();

            //Test an empty value can be retrieved
            var status = parameters.Status;
            Assert.IsTrue(status == null, "Status was not null");

            //Test a value can be set
            parameters.Status = new[] { Status.Up };
            Assert.IsTrue(parameters.Status.Length == 1 && parameters.Status.First() == Status.Up, "Status was not up");

            //Test a value can be overwritten
            parameters.Status = new[] { Status.Down };
            Assert.IsTrue(parameters.Status.Length == 1 && parameters.Status.First() == Status.Down, "Status was not down");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SensorParameters_HttpSensor_CanBeGetAndSet()
        {
            var parameters = new HttpSensorParameters();

            SetAndGet(parameters, nameof(HttpSensorParameters.Timeout), 300);
            SetAndGet(parameters, nameof(HttpSensorParameters.Url), "http://localhost");
            SetAndGet(parameters, nameof(HttpSensorParameters.HttpRequestMethod), HttpRequestMethod.HEAD);
            SetAndGet(parameters, nameof(HttpSensorParameters.PostData), "test");
            SetAndGet(parameters, nameof(HttpSensorParameters.UseCustomPostContent), true);
            SetAndGet(parameters, nameof(HttpSensorParameters.PostContentType), "stuff");
            SetAndGet(parameters, nameof(HttpSensorParameters.UseSNIFromUrl), true);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SensorParameters_FactorySensor_CanBeGetAndSet()
        {
            var parameters = new FactorySensorParameters(Enumerable.Empty<string>());
            SetAndGetArray(parameters, nameof(FactorySensorParameters.ChannelDefinition), "first", "second");
            SetAndGet(parameters, nameof(FactorySensorParameters.FactoryErrorMode), FactoryErrorMode.WarnOnError);
            SetAndGet(parameters, nameof(FactorySensorParameters.FactoryErrorFormula), "test");
            SetAndGet(parameters, nameof(FactorySensorParameters.FactoryMissingDataMode), FactoryMissingDataMode.CalculateWithZero);
        }

        private void SetAndGet(IParameters parameters, string property, object value)
        {
            var prop = parameters.GetType().GetProperty(property);

            if (prop == null)
                throw new ArgumentException($"Could not find property '{property}'");

            prop.SetValue(parameters, value);

            var val = prop.GetValue(parameters);

            Assert.AreEqual(value, val);
        }

        private void SetAndGetArray<T>(IParameters parameters, string property, params T[] value)
        {
            var prop = parameters.GetType().GetProperty(property);

            if (prop == null)
                throw new ArgumentException($"Could not find property '{property}'");

            prop.SetValue(parameters, value);

            var val = prop.GetValue(parameters);

            AssertEx.AreEqualLists(value?.ToList(), val?.ToIEnumerable().Cast<T>().ToList(), $"Property '{property}' was incorrect");
        }

        #endregion
        #region LogParameters

        [TestMethod]
        [TestCategory("UnitTest")]
        public void LogParameters_Date_CanBeGetAndSet()
        {
            var parameters = new LogParameters(null);

            var startDate = parameters.StartDate;
            Assert.IsTrue(startDate == null, "Status was not null");

            var date = DateTime.Now;
            parameters.StartDate = date;
            Assert.IsTrue(parameters.StartDate.ToString() == date.ToString(), $"Status was not {date}");

            var tomorrowStart = DateTime.Now.AddDays(1);
            var tomorrowEnd = DateTime.Now.AddDays(1).AddHours(3);
            parameters.StartDate = tomorrowStart;
            parameters.EndDate = tomorrowEnd;
            Assert.IsTrue(parameters.EndDate.ToString() == tomorrowEnd.ToString(), $"Updated start status was not {date}");
            Assert.IsTrue(parameters.EndDate.ToString() == tomorrowEnd.ToString(), $"Updated end status was not {date}");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void LogParameters_SetsRecordAge_InConstructor()
        {
            var parameters = new LogParameters(1001, RecordAge.LastSixMonths);

            Assert.AreEqual(parameters.RecordAge, RecordAge.LastSixMonths);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void LogParameters_SetsStartAndEnd_InConstructor()
        {
            var start = DateTime.Now;
            var end = DateTime.Now.AddDays(1);

            var parameters = new LogParameters(null, start, end);

            Assert.AreEqual(start.ToString(), parameters.StartDate.ToString(), "Start was not correct");
            Assert.AreEqual(end.ToString(), parameters.EndDate.ToString(), "End was not correct");
        }

        #endregion
        #region NewSensorParameters

        [TestMethod]
        [TestCategory("UnitTest")]
        public void NewSensorParameters_Enum_CanBeSet()
        {
            var parameters = new ExeXmlSensorParameters("test.ps1")
            {
                Priority = Priority.Three
            };

            Assert.AreEqual(Priority.Three, parameters.Priority);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void NewSensorParameters_Enum_CanBeSetToNull()
        {
            var parameters = new ExeXmlSensorParameters("test.ps1")
            {
                Priority = null
            };

            Assert.AreEqual(null, parameters.Priority);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void NewSensorParameters_Enum_Throws_WhenSetNotEnum()
        {
            var parameters = new FakeSensorParameters();

            AssertEx.Throws<InvalidCastException>(() => parameters.RestartStage = 1, "Unable to cast object of type 'System.Int32' to type 'System.Enum'");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void NewSensorParameters_EnablesDependentProperties()
        {
            var parameters = new ExeXmlSensorParameters("test.ps1");
            Assert.AreEqual(true, parameters.InheritInterval);

            parameters.Interval = ScanningInterval.FiveMinutes;

            Assert.AreEqual(ScanningInterval.FiveMinutes, parameters.Interval);
            Assert.AreEqual(false, parameters.InheritInterval);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void NewSensorParameters_AllPropertiesHavePropertyParameterAttributes()
        {
            var properties = typeof(NewSensorParameters).Assembly.GetTypes()
                .Where(t => typeof(NewSensorParameters).IsAssignableFrom(t))
                .SelectMany(t => t.GetNormalProperties())
                .Where(p => p.ReflectedType == p.DeclaringType)
                .OrderBy(p => p.Name)
                .ToList();

            var propertiesWithoutAttributes =
                properties.Where(p => p.GetCustomAttribute<PropertyParameterAttribute>() == null).OrderBy(p => p.DeclaringType.Name).ToList();

            var excludedProperties = new[]
            {
                Tuple.Create(typeof(DynamicSensorParameters), nameof(DynamicSensorParameters.Targets)),
                Tuple.Create(typeof(DynamicSensorParameters), nameof(DynamicSensorParameters.Source)),
                Tuple.Create(typeof(NewSensorParameters), nameof(NewSensorParameters.DynamicType)),
                Tuple.Create(typeof(RawSensorParameters), nameof(RawSensorParameters.Parameters)),
                Tuple.Create(typeof(SensorParametersInternal), nameof(SensorParametersInternal.SensorType)),
                Tuple.Create(typeof(SensorParametersInternal), nameof(SensorParametersInternal.Source))
            };

            propertiesWithoutAttributes = propertiesWithoutAttributes.Where(p => !excludedProperties.Any(e => p.DeclaringType == e.Item1 && p.Name == e.Item2)).ToList();

            if (propertiesWithoutAttributes.Count > 0)
            {
                var str = string.Join(", ", propertiesWithoutAttributes.Select(p => $"{p.DeclaringType}.{p.Name}"));

                Assert.Fail($"Properties {str} are missing a {nameof(PropertyParameterAttribute)}");
            }
        }

        #endregion
        #region RawSensorParameters

        [TestMethod]
        [TestCategory("UnitTest")]
        public void RawSensorParameters_Parameters_InitializesIfNull()
        {
            var parameters = new RawSensorParameters("testName", "sensorType")
            {
                [Parameter.Custom] = null
            };

            Assert.AreEqual(typeof (List<CustomParameter>), parameters.Parameters.GetType());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void RawSensorParameters_CanBeUsedAsDictionary()
        {
            var parameters = new RawSensorParameters("testName", "sensorType");

            parameters["customParam"] = 3;
            Assert.AreEqual(3, parameters["customParam"]);

            parameters["customParam_"] = 4;
            Assert.AreEqual(4, parameters["customParam_"]);

            Assert.AreNotEqual(parameters["customParam"], parameters["customParam_"]);

            Assert.IsTrue(parameters.Contains("customParam"));

            parameters["CUSTOMPARAM"] = 5;
            Assert.AreEqual(5, parameters["CUSTOMPARAM"]);
            Assert.AreEqual(5, parameters["customParam"]);

            Assert.IsTrue(parameters.Contains("customParam_"));

            parameters.Remove("customParam_");

            Assert.IsFalse(parameters.Contains("customParam_"));
            AssertEx.Throws<InvalidOperationException>(() =>
            {
                var val = parameters["customParam_"];
            }, "Parameter with name 'customParam_' does not exist.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void RawSensorParameters_WithoutPSObjectUtilities_SingleObject()
        {
            TestHelpers.WithPSObjectUtilities(() =>
            {
                var parameters = new RawSensorParameters("first", "second");

                var val = true;

                parameters["third"] = val;
                Assert.AreEqual(val, parameters["third"]);

                Assert.IsInstanceOfType(parameters.Parameters.First(p => p.Name == "third").Value, typeof(SimpleParameterContainerValue));

                var url = PrtgRequestMessageTests.CreateUrl(parameters);

                Assert.AreEqual("name_=first&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&third=True&sensortype=second", url);
            }, new DefaultPSObjectUtilities());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void RawSensorParameters_WithoutPSObjectUtilities_ObjectArray()
        {
            TestHelpers.WithPSObjectUtilities(() =>
            {
                var parameters = new RawSensorParameters("first", "second");

                var arr = new[] { 1, 2 };

                parameters["third"] = arr;
                Assert.AreEqual(arr, parameters["third"]);

                Assert.IsInstanceOfType(parameters.Parameters.First(p => p.Name == "third").Value, typeof(SimpleParameterContainerValue));

                var url = PrtgRequestMessageTests.CreateUrl(parameters);

                Assert.AreEqual("name_=first&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&third=1&third=2&sensortype=second", url);
            }, new DefaultPSObjectUtilities());
        }

        #endregion
        #region SensorHistoryParameters

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SensorHistoryParameters_GetsProperties()
        {
            var start = DateTime.Now;
            var parameters = new SensorHistoryParameters(1001, 600, null, null, null);

            Assert.AreEqual(parameters.StartDate.ToString(), start.ToString());
            Assert.AreEqual(parameters.EndDate.ToString(), start.AddHours(-1).ToString());
            Assert.AreEqual(parameters.Average, 600);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SensorHistoryParameters_Throws_WhenAverageIsLessThanZero()
        {
            AssertEx.Throws<ArgumentException>(() => new SensorHistoryParameters(1001, -1, null, null, null), "Average must be greater than or equal to 0");
        }

        #endregion
        #region NewDeviceParameters

        [TestMethod]
        [TestCategory("UnitTest")]
        public void NewDeviceParameters_SwapsHostWithIPVersion()
        {
            var parameters = new NewDeviceParameters("device", "dc-1");
            Assert.AreEqual("dc-1", GetCustomParameter(parameters, "host_"));
            Assert.AreEqual("dc-1", parameters.Host);

            parameters.IPVersion = IPVersion.IPv6;
            Assert.AreEqual("dc-1", GetCustomParameter(parameters, "hostv6_"));
            Assert.AreEqual(null, GetCustomParameter(parameters, "host_"));
            Assert.AreEqual("dc-1", parameters.Host);

            parameters.IPVersion = IPVersion.IPv4;
            Assert.AreEqual("dc-1", GetCustomParameter(parameters, "host_"));
            Assert.AreEqual(null, GetCustomParameter(parameters, "hostv6_"));
            Assert.AreEqual("dc-1", parameters.Host);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void NewDeviceParameters_AssignsHostToCorrectProperty()
        {
            var parameters = new NewDeviceParameters("device", "dc-1");
            Assert.AreEqual("dc-1", GetCustomParameter(parameters, "host_"));

            parameters.Host = "dc-2";
            Assert.AreEqual("dc-2", GetCustomParameter(parameters, "host_"));

            parameters.IPVersion = IPVersion.IPv6;
            Assert.AreEqual("dc-2", GetCustomParameter(parameters, "hostv6_"));

            parameters.Host = "dc-3";
            Assert.AreEqual("dc-3", GetCustomParameter(parameters, "hostv6_"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void NewDeviceParameters_SetsAutomaticTemplate_WhenTemplatesAssigned()
        {
            var templates = Execute(c => c.GetDeviceTemplates());

            var parameters = new NewDeviceParameters("dc-1");
            Assert.AreEqual(AutoDiscoveryMode.Manual, parameters.AutoDiscoveryMode);

            parameters.DeviceTemplates = templates;
            Assert.AreEqual(templates, parameters.DeviceTemplates);
            Assert.AreEqual(AutoDiscoveryMode.AutomaticTemplate, parameters.AutoDiscoveryMode);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void NewDeviceParameters_DoesNotChangeAutoDiscoveryMode_WhenNoTemplatesAssigned()
        {
            var parameters = new NewDeviceParameters("dc-1");
            Assert.AreEqual(AutoDiscoveryMode.Manual, parameters.AutoDiscoveryMode);

            parameters.DeviceTemplates = null;
            Assert.AreEqual(AutoDiscoveryMode.Manual, parameters.AutoDiscoveryMode);

            parameters.DeviceTemplates = new List<DeviceTemplate>();
            Assert.AreEqual(AutoDiscoveryMode.Manual, parameters.AutoDiscoveryMode);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void NewDeviceParameters_ClearsTemplates_WhenAutoDiscoveryModeChanged()
        {
            var templates = Execute(c => c.GetDeviceTemplates());

            var parameters = new NewDeviceParameters("dc-1");
            Assert.AreEqual(AutoDiscoveryMode.Manual, parameters.AutoDiscoveryMode);

            parameters.DeviceTemplates = templates;
            Assert.AreEqual(templates, parameters.DeviceTemplates);
            Assert.AreEqual(AutoDiscoveryMode.AutomaticTemplate, parameters.AutoDiscoveryMode);

            parameters.AutoDiscoveryMode = AutoDiscoveryMode.AutomaticTemplate;
            Assert.AreEqual(templates, parameters.DeviceTemplates);

            parameters.AutoDiscoveryMode = AutoDiscoveryMode.Automatic;
            Assert.AreEqual(null, parameters.DeviceTemplates);
        }

        #endregion
        #region PageableParameters

        [TestMethod]
        [TestCategory("UnitTest")]
        public void PageableParameters_IncreasesPage_StartAtZero()
        {
            var parameters = new SensorParameters();

            //No initial value
            Assert.AreEqual(null, parameters.Start, "Initial start was incorrect");
            Assert.AreEqual(null, parameters.Count, "Initial count was incorrect");
            Assert.AreEqual(1, parameters.Page);

            //Increasing the page when we have no count does nothing
            parameters.Page++;
            Assert.AreEqual(null, parameters.Start, "Start was affected after increasing page with no count");
            Assert.AreEqual(1, parameters.Page, "Page was affected after increasing page with no count");

            //Increasing the page when we have a count works
            parameters.Count = 500;
            parameters.Page++;
            Assert.AreEqual(500, parameters.Start, "Start after increasing page with count was incorrect");
            Assert.AreEqual(2, parameters.Page, "Page after increasing page with count was incorrect");

            //Decreasing the page sets the start to 0
            parameters.Page--;
            Assert.AreEqual(0, parameters.Start, "Start after decreasing page with count was incorrect");
            Assert.AreEqual(1, parameters.Page, "Page after decreasing page with count was incorrect");

            //Manually specifying the page number works
            parameters.Page = 3;
            Assert.AreEqual(1000, parameters.Start);
            Assert.AreEqual(3, parameters.Page);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void PageableParameters_IncreasesPage_StartAtOne()
        {
            var parameters = new LogParameters(null);

            //No initial value
            Assert.AreEqual(1, parameters.Start, "Initial start was incorrect");
            Assert.AreEqual(null, parameters.Count, "Initial count was incorrect");
            Assert.AreEqual(1, parameters.Page);

            //Increasing the page when we have no count does nothing
            parameters.Page++;
            Assert.AreEqual(1, parameters.Start, "Start was affected after increasing page with no count");
            Assert.AreEqual(1, parameters.Page, "Page was affected after increasing page with no count");

            //Increasing the page when we have a count works
            parameters.Count = 500;
            parameters.Page++;
            Assert.AreEqual(501, parameters.Start, "Start after increasing page with count was incorrect");
            Assert.AreEqual(2, parameters.Page, "Page after increasing page with count was incorrect");

            //Decreasing the page sets the start to our initial start
            parameters.Page--;
            Assert.AreEqual(1, parameters.Start, "Start after decreasing page with count was incorrect");
            Assert.AreEqual(1, parameters.Page, "Page after decreasing page with count was incorrect");

            //Manually specifying the page number works
            parameters.Page = 3;
            Assert.AreEqual(1001, parameters.Start);
            Assert.AreEqual(3, parameters.Page);
        }

        #endregion
        #region ProbeParameters

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ProbeParameters_SearchFilter_ParentId_Equals_0()
        {
            var parameters = new ProbeParameters(new SearchFilter(Property.ParentId, 0));

            var url = PrtgRequestMessageTests.CreateUrl(parameters, false);

            Assert.AreEqual(TestHelpers.RequestProbe("count=*&filter_parentid=0", UrlFlag.Columns), url);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ProbeParameters_SearchFilter_ParentId_NotEquals_0()
        {
            AssertEx.Throws<InvalidOperationException>(
                () => new ProbeParameters(new SearchFilter(Property.ParentId, FilterOperator.NotEquals, 0)),
                "Cannot filter for probes based on a ParentId other than 0."
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ProbeParameters_SearchFilter_ParentId_Equals_1()
        {
            AssertEx.Throws<InvalidOperationException>(
                () => new ProbeParameters(new SearchFilter(Property.ParentId, 1)),
                "Cannot filter for probes based on a ParentId other than 0."
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ProbeParameters_SearchFilter_ParentId_Equals_ArrayWith_0()
        {
            var parameters = new ProbeParameters(new SearchFilter(Property.ParentId, new[] {0}));

            var url = PrtgRequestMessageTests.CreateUrl(parameters, false);

            Assert.AreEqual(TestHelpers.RequestProbe("count=*&filter_parentid=0", UrlFlag.Columns), url);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ProbeParameters_SearchFilter_ParentId_Equals_ArrayWithout_0()
        {
            AssertEx.Throws<InvalidOperationException>(
                () => new ProbeParameters(new SearchFilter(Property.ParentId, new[] {1})),
                "Cannot filter for probes based on a ParentId other than 0."
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ProbeParameters_SearchFilter_ParentId_Equals_ArrayWith_0_AndSomethingElse()
        {
            AssertEx.Throws<InvalidOperationException>(
                () => new ProbeParameters(new SearchFilter(Property.ParentId, new[] { 0, 1 })),
                "Cannot filter for probes based on a ParentId other than 0."
            );
        }

        #endregion
        #region GetObjectPropertyRawParameters

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GetObjectPropertyRawParameters_SpecifiesShow_WithStringProperty()
        {
            Execute(
                c => c.GetObjectProperty(1001, ObjectProperty.Name),
                "id=1001&name=name&show=text&username=username"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GetObjectPropertyRawParameters_DoesNotSpecifyShow_WithNonStringProperty()
        {
            Execute(
                c => c.GetObjectProperty(1001, ObjectProperty.Active),
                "id=1001&name=active&username=username"
            );
        }

        #endregion
        #region SetChannelPropertyParameters

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetChannelPropertyParameters_ConstructorValidation()
        {
            var settings = new[] { new ChannelParameter(ChannelProperty.LimitsEnabled, true) };

            AssertEx.Throws<ArgumentNullException>(() => new SetChannelPropertyParameters(null, 1, settings), "Value cannot be null.\r\nParameter name: sensorIds");
            AssertEx.Throws<ArgumentException>(() => new SetChannelPropertyParameters(new int[] { }, 1, settings), "At least one Sensor ID must be specified.\r\nParameter name: sensorIds");
            AssertEx.Throws<ArgumentNullException>(() => new SetChannelPropertyParameters(new[] { 1 }, 1, null), "Value cannot be null.\r\nParameter name: parameters");
            AssertEx.Throws<ArgumentException>(() => new SetChannelPropertyParameters(new[] { 1 }, 1, new ChannelParameter[] { }), "At least one parameter must be specified.\r\nParameter name: parameters");
        }

        #endregion

        private string GetCustomParameter(BaseParameters parameters, string name)
        {
            var customParameters = ((List<CustomParameter>) parameters.GetParameters()[Parameter.Custom]);

            var targetParameter = customParameters.FirstOrDefault(p => p.Name == name);

            return targetParameter?.Value?.ToString();
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Parameters_ReplacesCounterpart()
        {
            var parameters = new BaseParameters
            {
                [Parameter.Password] = "password",
                [Parameter.PassHash] = "passhash"
            };

            Assert.AreEqual(1, parameters.GetParameters().Keys.Count);
            Assert.AreEqual(parameters[Parameter.PassHash], "passhash");
            Assert.AreEqual(null, parameters[Parameter.Password]);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void AllParameterProperties_CanSetAndRetrieveNull()
        {
            var groups = typeof(BaseParameters).Assembly.GetTypes()
                .Where(t => typeof(BaseParameters).IsAssignableFrom(t) && !t.IsAbstract)
                .SelectMany(t => t.GetNormalProperties())
                .OrderBy(p => p.Name)
                .GroupBy(p => p.ReflectedType)
                .OrderBy(p => p.Key.Name)
                .ToList();

            foreach (var group in groups)
            {
                var instance = GetParametersInstance(group.Key);

                foreach (var property in group)
                {
                    var p = property;

                    if (group.Key.IsGenericType)
                    {
                        p = instance.GetType().GetProperty(property.Name);
                    }

                    if (p.PropertyType.IsValueType)
                        continue;

                    try
                    {
                        p.SetValue(instance, null);
                    }
                    catch (Exception ex)
                    {
                        if (group.Key.Name == "SpeedTriggerParameters" && p.Name == "Channel" && ex.Message ==
                            "Trigger property 'Channel' cannot be null for trigger type 'Speed'.")
                        {
                            continue;
                        }
                    }

                    var val = p.GetValue(instance);
                }
            }
        }

        internal static IParameters GetParametersInstance(Type type)
        {
            if (type.Name == "SystemInfoParameters`1")
                type = typeof(SystemInfoParameters<DeviceSystemInfo>);

            var ctor = type.GetConstructors().FirstOrDefault();

            if (ctor != null)
            {
                var args = PrtgClientTests.GetParameters(ctor);

                return (IParameters)Activator.CreateInstance(type, args);
            }
            else
            {
                switch (type.Name)
                {
                    case nameof(DynamicSensorParameters):
                        return new DynamicSensorParameters("<input name=\"name_\" value=\"test\">", "exexml");

                    default:
                        throw new NotImplementedException($"Don't know how to create instance of parameters type '{type.Name}'");
                }
            }
        }
    }
}
