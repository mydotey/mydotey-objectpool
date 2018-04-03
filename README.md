# MyDotey ObjectPool

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

MyDotey ObjectPool, a generic object pool, implementatin based on ConcurrentHashMap & BlockingQueue, easy to use.

Implemented in java/dotnet separately. Code is clean. Easy to read.

ThreadPool & AutoScaleThreadPool are implemented based on the ObjectPool & AutoScaleObjectPool as well.

## Usage

* java
  https://github.com/mydotey/mydotey-objectpool/tree/master/java

* dotnet
  https://github.com/mydotey/mydotey-objectpool/tree/master/dotnet

## Features

* ObjectPool
  * acquire, get an object, if no object, block the call

  * tryAcquire, get an object, if no object, get null

  * release, return an object to pool

* AutoScaleObjectPool
  * self-refreshing automatically

  * close idle objects automatically

  * recognize stale object and replace with new none automatically

  * scale out batch objects

* ThreadPool
  * implementation based on object pool

* AutoScaleThreadPool
  * implementation based on auto scale object pool

## Configuration

* ObjectPool
  * minSize, min size objects in pool, default to 0

  * maxSize, max size objects in pool, required

  * objectFactory, object creation logic, required

  * onCreate, object creation hook, default to no hook

  * onClose, object close hook, the default behavior is to invoke close/dispose method automatically for Closeable/Disposable objects

* AutoScaleObjectPool

  * objectTtl, unit ms, object max live time, if reached, remove it & create a new one, default to long max

  * maxIdleTime, unit ms, max idle time, if reached, remove it, default to long Max

  * staleChecker, object stale decision maker, if one is stale, remove it & replace with a new one, default to always not stale

  * checkInterval, unit ms, default to 10 * 1000 ms

  * scaleFactor, the batch size when scaling out, default to 1

* ThreadPool
  * queueCapacity, size limit of the task queue, if set to 0, no queue is used, default to int max

## Developers

* Qiang Zhao <koqizhao@outllook.com>

