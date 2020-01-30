#pragma once

#include <vector>

namespace event
{
    // composition style
    template <typename TArgs>
    struct EventHandler
    {
        using ArgType = TArgs;

        virtual         ~EventHandler() = default;

        virtual void    operator() (void * sender, const TArgs & args) = 0;

    };
    template <>
    struct EventHandler<void>
    {
        using ArgType = void;

        virtual         ~EventHandler() = default;

        virtual void    operator() (void * sender) = 0;
    };
    // inheritance style
    template <typename TReceiver, typename TEventHandler, typename TArgs = typename TEventHandler::ArgType>
    struct EventHandlerClass : public TEventHandler
    {
        static_assert(std::is_base_of_v<EventHandler<TArgs>, TEventHandler>,
                      "Must inherit from EventHandler or its subclass.");

        using FuncType = void (TReceiver::*)(void * sender, const TArgs & args);

        virtual void    operator() (void * sender, const TArgs & args) override
        {
            (m_receiver->*m_handleFunc)(sender, args);
        }

        TReceiver *     m_receiver;
        FuncType        m_handleFunc;
    };
    template <typename TReceiver, typename TEventHandler>
    struct EventHandlerClass<TReceiver, TEventHandler, void> : public TEventHandler
    {
        static_assert(std::is_base_of_v<EventHandler<void>, TEventHandler>,
                      "Must inherit from EventHandler or its subclass.");

        using FuncType = void (TReceiver::*)(void * sender);

        virtual void    operator() (void * sender) override
        {
            (m_receiver->*m_handleFunc)(sender);
        }

        TReceiver *     m_receiver;
        FuncType        m_handleFunc;
    };
    template <typename TArgs>
    class Event
    {
    public:
        using EventHandlerRef = EventHandler<TArgs> &;
        using EventHandlerPtr = EventHandler<TArgs> *;

        void            AddHandler(EventHandlerRef handler) { handlers.emplace_back(&handler); }
        void            RemoveHandler(EventHandlerRef handler)
        {
            for (auto it = handlers.rbegin(); it != handlers.rend(); ++it)
            {
                if (*it == &handler)
                {
                    handlers.erase(std::next(it).base());
                    break;
                }
            }
        }

        void            Dispatch(void * sender, const TArgs & args) { for (auto h : handlers) (*h)(sender, args); }

    private:
        std::vector<EventHandlerPtr> handlers;
    };
    template <>
    class Event<void>
    {
    public:
        using EventHandlerRef = EventHandler<void> &;
        using EventHandlerPtr = EventHandler<void> *;

        void            AddHandler(EventHandlerRef handler) { handlers.emplace_back(&handler); }
        void            RemoveHandler(EventHandlerRef handler)
        {
            for (auto it = handlers.rbegin(); it != handlers.rend(); ++it)
            {
                if (*it == &handler)
                {
                    handlers.erase(std::next(it).base());
                    break;
                }
            }
        }

        void            Dispatch(void * sender) { for (auto h : handlers) (*h)(sender); }

    private:
        std::vector<EventHandlerPtr> handlers;
    };

// --------------------------------------------------------------------------
// Define Events
// --------------------------------------------------------------------------

#define _DEFINE_EVENT(name) \
    struct name##EventHandler : ::event::EventHandler<void> { using ::event::EventHandler<void>::EventHandler; }; \
    template <typename TReceiver> class name##EventHandlerClass : public ::event::EventHandlerClass<TReceiver, name##EventHandler> {}; \
    struct name##Event : ::event::Event<void> {};
#define _DEFINE_EVENT1(name, args) \
    struct name##EventHandler : ::event::EventHandler<args> { using ::event::EventHandler<args>::EventHandler; }; \
    template <typename TReceiver> class name##EventHandlerClass : public ::event::EventHandlerClass<TReceiver, name##EventHandler> {}; \
    struct name##Event : ::event::Event<args> {};

// --------------------------------------------------------------------------
// Define Senders, Receivers, and Bind
// --------------------------------------------------------------------------

#define _SEND_EVENT(name) \
    private: \
        name##Event __##name##_event_object; \
    public: \
        name##Event & Get##name##Event() { return __##name##_event_object; }

// composition style
#define _RECV_EVENT_DECL(name) \
    private: \
        struct __##name##EventHandlerImpl : public name##EventHandler \
        { \
            virtual void    operator() (void * sender) override; \
        } __##name##_event_handler; \
    public: \
        name##EventHandler & Get##name##EventHandler() { return __##name##_event_handler; }
#define _RECV_EVENT_DECL1(name) \
    private: \
        struct __##name##EventHandlerImpl : public name##EventHandler \
        { \
            virtual void    operator() (void * sender, const name##EventHandler::ArgType & args) override; \
        } __##name##_event_handler; \
    public: \
        name##EventHandler & Get##name##EventHandler() { return __##name##_event_handler; }
#define _RECV_EVENT_IMPL(receiver, name) \
    void receiver::__##name##EventHandlerImpl::operator()

// inheritance style
#define _IS_EVENT_HANDLER(handler, name) \
    private name##EventHandlerClass<handler>
#define _HANDLE_EVENT_DECL(name) \
    name##EventHandler & Get##name##EventHandler() \
    { \
        using ThisType = std::remove_reference<decltype(*this)>::type; \
        using EventHandlerClassType = name##EventHandlerClass<ThisType>; \
        /* should only assign once */ \
        EventHandlerClassType::m_receiver = this; \
        EventHandlerClassType::m_handleFunc = &ThisType::__##name##EventHandleFunc; \
        return static_cast<EventHandlerClassType&>(*this); \
    } \
    void __##name##EventHandleFunc(void * sender);
#define _HANDLE_EVENT_DECL1(name) \
    name##EventHandler & Get##name##EventHandler() \
    { \
        using ThisType = std::remove_reference<decltype(*this)>::type; \
        using EventHandlerClassType = name##EventHandlerClass<ThisType>; \
        /* should only assign once */ \
        EventHandlerClassType::m_receiver = this; \
        EventHandlerClassType::m_handleFunc = &ThisType::__##name##EventHandleFunc; \
        return static_cast<EventHandlerClassType&>(*this); \
    } \
    void __##name##EventHandleFunc(void * sender, const name##EventHandler::ArgType & args);
#define _HANDLE_EVENT_IMPL(handler, name) \
    void handler::__##name##EventHandleFunc

#define _BIND_EVENT(name, sender, receiver) \
    do { \
        (sender).Get##name##Event().AddHandler((receiver).Get##name##EventHandler()); \
    } while (0)
#define _UNBIND_EVENT(name, sender, receiver) \
    do { \
        (sender).Get##name##Event().RemoveHandler((receiver).Get##name##EventHandler()); \
    } while (0)

// --------------------------------------------------------------------------
// Generate Events
// --------------------------------------------------------------------------

#define _DISPATCH_EVENT(name, sender) \
    do { \
        (sender).Get##name##Event().Dispatch(&(sender)); \
    } while (0)
#define _DISPATCH_EVENT1(name, sender, args) \
    do { \
        (sender).Get##name##Event().Dispatch(&(sender), (args)); \
    } while (0)
    
// --------------------------------------------------------------------------
// Usage
// --------------------------------------------------------------------------
// #define _TEST_EVENT_USAGE_
#ifdef _TEST_EVENT_USAGE_

    // #include <sstream>

    _DEFINE_EVENT(NoArg)
    _DEFINE_EVENT1(IntArg, int)
    struct ComplexArgs
    {
        int x, y, z;
    };
    _DEFINE_EVENT1(CpxArg, ComplexArgs)

    struct Foo
    {
        const char *    name = "Foo";

        _SEND_EVENT(NoArg)
        _SEND_EVENT(IntArg)
        _SEND_EVENT(CpxArg)
    };

    struct Bar
    {
        _RECV_EVENT_DECL(NoArg)
        _RECV_EVENT_DECL1(IntArg)
        _RECV_EVENT_DECL1(CpxArg)
    };

    struct Joo
        : _IS_EVENT_HANDLER(Joo, NoArg)
        , _IS_EVENT_HANDLER(Joo, IntArg)
        , _IS_EVENT_HANDLER(Joo, CpxArg)
    {
        _HANDLE_EVENT_DECL(NoArg)
        _HANDLE_EVENT_DECL1(IntArg)
        _HANDLE_EVENT_DECL1(CpxArg)
    };

    _RECV_EVENT_IMPL(Bar, NoArg) (void * sender)
    {
        std::wostringstream ss;
        ss << "Receive NoArg event from " << reinterpret_cast<Foo *>(sender)->name << std::endl;
        OutputDebugString(ss.str().c_str());
    }
    _RECV_EVENT_IMPL(Bar, IntArg) (void * sender, const int & args)
    {
        std::wostringstream ss;
        ss << "Receive IntArg event from " << reinterpret_cast<Foo *>(sender)->name << " args = " << args << std::endl;
        OutputDebugString(ss.str().c_str());
    }
    _RECV_EVENT_IMPL(Bar, CpxArg) (void * sender, const ComplexArgs & args)
    {
        std::wostringstream ss;
        ss << "Receive CpxArg event from " << reinterpret_cast<Foo *>(sender)->name << " args = {" << args.x << "," << args.y << "," << args.z << "}" << std::endl;
        OutputDebugString(ss.str().c_str());
    }
    _HANDLE_EVENT_IMPL(Joo, NoArg) (void * sender)
    {
        std::wostringstream ss;
        ss << "Receive NoArg event from " << reinterpret_cast<Foo *>(sender)->name << std::endl;
        OutputDebugString(ss.str().c_str());
    }
    _HANDLE_EVENT_IMPL(Joo, IntArg) (void * sender, const int & args)
    {
        std::wostringstream ss;
        ss << "Receive IntArg event from " << reinterpret_cast<Foo *>(sender)->name << " args = " << args << std::endl;
        OutputDebugString(ss.str().c_str());
    }
    _HANDLE_EVENT_IMPL(Joo, CpxArg) (void * sender, const ComplexArgs & args)
    {
        std::wostringstream ss;
        ss << "Receive CpxArg event from " << reinterpret_cast<Foo *>(sender)->name << " args = {" << args.x << "," << args.y << "," << args.z << "}" << std::endl;
        OutputDebugString(ss.str().c_str());
    }

    void Usage()
    {
        Foo foo;
        Bar bar;
        Joo joo;

        _BIND_EVENT(NoArg, foo, bar);
        _BIND_EVENT(IntArg, foo, bar);
        _BIND_EVENT(CpxArg, foo, bar);

        struct ComplexArgs ca = { 11, 22, 33 };
        _DISPATCH_EVENT(NoArg, foo);
        _DISPATCH_EVENT1(IntArg, foo, 123);
        _DISPATCH_EVENT1(CpxArg, foo, ca);

        _UNBIND_EVENT(NoArg, foo, bar);
        _DISPATCH_EVENT(NoArg, foo);
        _DISPATCH_EVENT1(IntArg, foo, 123);
        _DISPATCH_EVENT1(CpxArg, foo, ca);
        
        _UNBIND_EVENT(IntArg, foo, bar);
        _DISPATCH_EVENT(NoArg, foo);
        _DISPATCH_EVENT1(IntArg, foo, 123);
        _DISPATCH_EVENT1(CpxArg, foo, ca);

        _UNBIND_EVENT(CpxArg, foo, bar);
        _DISPATCH_EVENT(NoArg, foo);
        _DISPATCH_EVENT1(IntArg, foo, 123);
        _DISPATCH_EVENT1(CpxArg, foo, ca);

        _BIND_EVENT(NoArg, foo, joo);
        _BIND_EVENT(IntArg, foo, joo);
        _BIND_EVENT(CpxArg, foo, joo);

        _DISPATCH_EVENT(NoArg, foo);
        _DISPATCH_EVENT1(IntArg, foo, 123);
        _DISPATCH_EVENT1(CpxArg, foo, ca);

        _UNBIND_EVENT(NoArg, foo, joo);
        _DISPATCH_EVENT(NoArg, foo);
        _DISPATCH_EVENT1(IntArg, foo, 123);
        _DISPATCH_EVENT1(CpxArg, foo, ca);

        _UNBIND_EVENT(IntArg, foo, joo);
        _DISPATCH_EVENT(NoArg, foo);
        _DISPATCH_EVENT1(IntArg, foo, 123);
        _DISPATCH_EVENT1(CpxArg, foo, ca);

        _UNBIND_EVENT(CpxArg, foo, joo);
        _DISPATCH_EVENT(NoArg, foo);
        _DISPATCH_EVENT1(IntArg, foo, 123);
        _DISPATCH_EVENT1(CpxArg, foo, ca);
    }

#endif
}