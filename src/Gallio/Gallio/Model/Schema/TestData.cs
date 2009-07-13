// Copyright 2005-2009 Gallio Project - http://www.gallio.org/
// Portions Copyright 2000-2004 Jonathan de Halleux
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Gallio.Common;
using Gallio.Common.Collections;
using Gallio.Common.Reflection;
using Gallio.Common.Xml;
using Gallio.Model;
using Gallio.Model.Schema;
using Gallio.Model.Tree;

namespace Gallio.Model.Schema
{
    /// <summary>
    /// Describes a test in a portable manner for serialization.
    /// </summary>
    /// <seealso cref="Test"/>
    [Serializable]
    [XmlRoot("test", Namespace = SchemaConstants.XmlNamespace)]
    [XmlType(Namespace = SchemaConstants.XmlNamespace)]
    public sealed class TestData : TestComponentData
    {
        private string fullName;
        private readonly List<TestData> children;
        private readonly List<TestParameterData> parameters;
        private bool isTestCase;

        /// <summary>
        /// Creates an uninitialized instance for Xml deserialization.
        /// </summary>
        private TestData()
        {
            children = new List<TestData>();
        }

        /// <summary>
        /// Creates a test info.
        /// </summary>
        /// <param name="id">The component id.</param>
        /// <param name="name">The component name.</param>
        /// <param name="fullName">The full name of the test.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="id"/>, <paramref name="name"/>,
        /// or <paramref name="fullName"/> is null.</exception>
        public TestData(string id, string name, string fullName)
            : base(id, name)
        {
            if (fullName == null)
                throw new ArgumentNullException(@"fullName");

            this.fullName = fullName;

            children = new List<TestData>();
            parameters = new List<TestParameterData>();
        }

        /// <summary>
        /// Copies the contents of a test.
        /// </summary>
        /// <param name="source">The source test.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null.</exception>
        public TestData(Test source)
            : base(source)
        {
            fullName = source.FullName;
            isTestCase = source.IsTestCase;

            children = new List<TestData>();
            parameters = new List<TestParameterData>();

            GenericCollectionUtils.ConvertAndAddAll(source.Children, children, delegate(Test child)
            {
                return new TestData(child);
            });

            GenericCollectionUtils.ConvertAndAddAll(source.Parameters, parameters, delegate(TestParameter parameter)
            {
                return new TestParameterData(parameter);
            });
        }

        /// <summary>
        /// Gets or sets the full name of the test.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        [XmlAttribute("fullName")]
        public string FullName
        {
            get { return fullName; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(@"value");
                fullName = value;
            }
        }

        /// <summary>
        /// Gets or sets whether this node is a test case.
        /// </summary>
        /// <seealso cref="Test.IsTestCase"/>
        [XmlAttribute("isTestCase")]
        public bool IsTestCase
        {
            get { return isTestCase; }
            set { isTestCase = value; }
        }

        /// <summary>
        /// Gets the mutable list of children.
        /// </summary>
        /// <seealso cref="Test.Children"/>
        [XmlArray("children", IsNullable = false)]
        [XmlArrayItem("test", typeof(TestData), IsNullable = false)]
        public List<TestData> Children
        {
            get { return children; }
        }

        /// <summary>
        /// Gets the mutable list of parameters.
        /// </summary>
        /// <seealso cref="Test.Parameters"/>
        [XmlArray("parameters", IsNullable = false)]
        [XmlArrayItem("parameter", typeof(TestParameterData), IsNullable = false)]
        public List<TestParameterData> Parameters
        {
            get { return parameters; }
        }

        /// <summary>
        /// Recursively enumerates this test and all of its descendants.
        /// </summary>
        [XmlIgnore]
        public IEnumerable<TestData> AllTests
        {
            get
            {
                return TreeUtils.GetPreOrderTraversal(this, test => test.Children);
            }
        }

        /// <summary>
        /// Recreates a <see cref="Test" /> object from the test data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Because a test data object does not contain all of the details of the original
        /// test some information may be lost in the round trip.
        /// </para>
        /// </remarks>
        /// <returns>The test.</returns>
        public Test ToTest()
        {
            var test = new Test(Name, CodeElement)
            {
                Id = Id,
                IsTestCase = IsTestCase
            };

            test.Metadata.AddAll(Metadata);
            Parameters.ForEach(x => test.AddParameter(x.ToTestParameter()));
            Children.ForEach(x => test.AddChild(x.ToTest()));
            return test;
        }
    }
}