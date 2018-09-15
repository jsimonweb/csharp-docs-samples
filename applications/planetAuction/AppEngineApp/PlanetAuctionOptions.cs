﻿/*
 * Copyright (c) 2017 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy of
 * the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations under
 * the License.
 */

namespace PlanetAuction
{
    public class PlanetAuctionOptions
    {
        public string BucketName { get; set; }
        public string ObjectName { get; set; } = "sample.txt";

        public string ProjectId { get; set; }
        public string InstanceId { get; set; }
        public string DatabaseId { get; set; }
    }
}
