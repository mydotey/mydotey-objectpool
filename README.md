mydotey objectpool
================

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

mydotey objectpool，1个通用的对象池，基于ConcurrentHashMap和BlockingQueue实现，代码简洁，容易理解。

提供了java和dotnet 2种实现。

更多产品介绍参见: https://github.com/mydotey/mydotey-objectpool/wiki

# Features
* 通用对象池：ObjectPool
  * acquire，获取1个对象，如果无可用对象，阻塞调用

  * tryAcquire，获取1个对象，如果无可用对象，返回null，不阻塞调用

  * release，释放对象

* 通用自伸缩对象池：AutoScaleObjectPool
  * 自动过期刷新

  * 自动闲置回收

  * 自动故障检测替换

  * 批量扩容

* 通用线程池：ThreadPool
  * 基于ObjectPool实现

* 通用自伸缩线程池：AutoScaleThreadPool
  * 基于AutoScaleObjectPool实现

# Configuration
* 对象池
  * minSize，最少保持对象数，默认为0

  * maxSize，最多保持对象数，必须配置

  * objectFactory，对象创建逻辑，必须配置

  * onCreate，对象创建后的扩展点，默认无操作

  * onClose，对象关闭时的扩展点，可用于释放对象资源，默认自动关闭Closeable/Disposable对象

* 自动伸缩对象池

  * objectTtl，对象最长生存时间，过期后，自动回收对象，重新创建1个新的对象，默认long.Max

  * maxIdleTime，对象最长闲置时间，过期后，自动回收对象，默认long.Max

  * staleChecker，对象损坏检测扩展点，可用于自定义故障检测逻辑，如果检测到损坏，自动回收对象，重现创建1个新的对象，默认总认为不stale

  * checkInterval，对象过期、闲置、损坏的定期检查间隔，默认10s

  * scaleFactor，当需要扩容时，批量扩容数目，默认为1

* 线程池
  * queueCapacity，任务队列大小限制，默认int.Max，如设置为0，表示不进行入缓冲队列

# Usage
* java
https://github.com/mydotey/mydotey-objectpool/tree/master/java

* dotnet
https://github.com/mydotey/mydotey-objectpool/tree/master/dotnet

# Developers
* Qiang Zhao <koqizhao@outllook.com>

