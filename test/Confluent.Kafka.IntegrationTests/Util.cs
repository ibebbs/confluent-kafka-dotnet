// Copyright 2016-2017 Confluent Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// Refer to LICENSE for more information.

using System;
using System.Text;
using System.Collections.Generic;
using Confluent.Kafka.Serialization;
using Xunit;


namespace Confluent.Kafka.IntegrationTests
{
    public static class Util
    {
        /// <summary>
        ///     Produces <param name="number"/> messages of size <param name="size"/>.
        /// </summary>
        /// <returns>
        ///     The offset of the first produced message.
        /// </returns>
        public static TopicPartitionOffset ProduceMessages(string bootstrapServers, string topic, int size, int number)
        {
            var producerConfig = new Dictionary<string, object> { { "bootstrap.servers", bootstrapServers } };

            var sb = new StringBuilder(size);
            for (int i=0; i<size; ++i)
            {
                // 01234 ... xyz0123 ...
                sb.Append((char)(0x30 + i%74));
            }
            var msg = sb.ToString();

            Message<Null, string> firstDeliveryReport = default(Message<Null, string>);
            using (var producer = new Producer<Null, string>(producerConfig, null, new StringSerializer(Encoding.UTF8)))
            {
                for (int i=0; i<number; ++i)
                {
                    var dr = producer.ProduceAsync(topic, null, msg).Result;
                    if (i == 0)
                    {
                        firstDeliveryReport = dr;
                    }
                    Assert.Equal(topic, dr.Topic);
                    Assert.NotEqual<long>(dr.Offset, Offset.Invalid);
                }

                producer.Flush();
            }

            return firstDeliveryReport.TopicPartitionOffset;
        }
    }
}