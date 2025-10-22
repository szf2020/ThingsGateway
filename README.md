# ThingsGateway

[![star](https://gitee.com/ThingsGateway/ThingsGateway/badge/star.svg?theme=gvp)](https://gitee.com/ThingsGateway/ThingsGateway/stargazers) 
[![star](https://img.shields.io/github/stars/ThingsGateway/ThingsGateway?logo=github)](https://github.com/ThingsGateway/ThingsGateway)
[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/ThingsGateway/ThingsGateway)﻿
[![NuGet(ThingsGateway)](https://img.shields.io/nuget/v/ThingsGateway.Foundation.svg?label=ThingsGateway)](https://www.nuget.org/packages/ThingsGateway.Foundation/)
[![NuGet(ThingsGateway)](https://img.shields.io/nuget/dt/ThingsGateway.Foundation.svg)](https://www.nuget.org/packages/ThingsGateway.Foundation/)
[![License](https://img.shields.io/badge/license-Apache%202-4EB1BA.svg)](https://thingsgateway.cn/docs/1)
<a href="http://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=NnBjPO-8kcNFzo_RzSbdICflb97u2O1i&authKey=V1MI3iJtpDMHc08myszP262kDykbx2Yev6ebE4Me0elTe0P0IFAmtU5l7Sy5w0jx&noverify=0&group_code=605534569">
<img src="https://img.shields.io/badge/QQ群-605534569-red" alt="QQ">
</a>

## Introduction
﻿
A cross-platform, high-performance edge data collection gateway based on net8/10.
﻿

## Documentation

﻿
[Documentation](https://thingsgateway.cn/).
﻿
[NuGet](https://www.nuget.org/packages?q=Tags%3A%22ThingsGateway%22)
﻿


## Demo

﻿
[Demo](https://demo.thingsgateway.cn/)

﻿
Account: **SuperAdmin**

﻿
Password: **111111**

﻿

## Docker

```shell

docker pull registry.cn-shenzhen.aliyuncs.com/thingsgateway/thingsgateway

docker pull registry.cn-shenzhen.aliyuncs.com/thingsgateway/thingsgateway_arm64
```



### Plugin List

﻿

#### Data Collection Plugins


| Plugin Name | Remarks                                                       |
| ----------- | ------------------------------------------------------------- |
| Modbus      | Supports Rtu/Tcp message formats, with Serial/Tcp/Udp links   |
| SiemensS7   | Siemens PLC S7 series                                         |
| Dlt6452007  | Supports Serial/Tcp/Udp links                                 |
| OpcDaMaster | Compiled for 64-bit                                           |
| OpcUaMaster | Supports certificate login, object extension, Json read/write |
| Webhook          | Webhook                                             |

#### Business Plugins


| Plugin Name      | Remarks                                                                                           |
| ---------------- | ------------------------------------------------------------------------------------------------- |
| ModbusSlave      | Supports Rtu/Tcp message formats, with Serial/Tcp/Udp links, supports Rpc reverse writing         |
| OpcUaServer      | OpcUa server, supports Rpc reverse writing                                                        |
| MqttClient       | Mqtt client, supports Rpc reverse writing, script-customizable upload content                     |
| MqttServer       | Mqtt server, supports WebSocket, supports Rpc reverse writing, script-customizable upload content |
| KafkaProducer    | Script-customizable upload content                                                                |
| RabbitMQProducer | Script-customizable upload content                                                                |
| SqlDB            | Relational database storage, supports historical storage and real-time data updates               |
| SqlHistoryAlarm      | Alarm historical data relational database storage                                                 |
| TDengineDB       | Time-series database storage                                                                      |
| QuestDB          | Time-series database storage                                                                      |

﻿

## License

﻿
[License](https://thingsgateway.cn/docs/1)
﻿
﻿

## Sponsorship

﻿
[Sponsorship Approach](https://thingsgateway.cn/docs/1000)
﻿

## Community

﻿
QQ Group: 605534569 [Jump](http://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=NnBjPO-8kcNFzo_RzSbdICflb97u2O1i&authKey=V1MI3iJtpDMHc08myszP262kDykbx2Yev6ebE4Me0elTe0P0IFAmtU5l7Sy5w0jx&noverify=0&group_code=605534569)
﻿

## Pro Plugins

﻿
[Plugin List](https://thingsgateway.cn/docs/1001)
