from multiprocessing import Pool
import multiprocessing
from datetime import datetime

counter = 0
def f(x):
    global counter
    counter = counter + 1
    #if x == 9 :
    #    raise NameError("error")
    return x*x

def count(x):
    while x > 0 :
        x-=1
    return x

def repeat(object, times=None):
    # repeat(10, 3) --> 10 10 10
    if times is None:
        while True:
            yield object
    else:
        for i in xrange(times):
            yield object

if __name__ == '__main__':
    print multiprocessing.cpu_count()
    start = datetime.now()
    pool = Pool(processes=4)              # start 4 worker processes
    end = datetime.now()
    print "create pool : " + str((end - start))
    f(2)
    print counter

    COUNT = 100000000

    start = datetime.now()
    result = pool.apply_async(count, [COUNT])    # evaluate "f(10)" asynchronously
    result.get(timeout=10)           # prints "100" unless your computer is *very* slow
    end = datetime.now()
    print "compute one value in parallel: " + str((end - start))
    print counter
    
    start = datetime.now()
    pool.map(count, repeat(COUNT, 4))          # prints "[0, 1, 4,..., 81]"
    end = datetime.now()
    print "compute 4 values in parallel : " + str((end - start))

    start = datetime.now()
    count(COUNT)
    end = datetime.now()
    print "compute one value in sequence : " + str((end - start))

    start = datetime.now()
    for i in repeat(COUNT, 4):
        count(i)
    end = datetime.now()
    print "compute 4 values in sequence : " + str((end - start))
