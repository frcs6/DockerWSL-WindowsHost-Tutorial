```
graph TD
subgraph Windows
  subgraph WSL
    subgraph Linux
        subgraph Docker tools
        D[Docker]
        DC[Docker Compose]
        end
        subgraph Containers
        C(Container)
        end
    end
  end
  A(Application)
  A-->C
  C-->A   
end
```