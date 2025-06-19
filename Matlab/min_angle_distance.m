function [out] = min_angle_distance(a,b)
    
    while(true)
        if(a < 0)
            a = a + 360;
            continue;
        end
        break;
    end

    while(true)
        if(b < 0)
            b = b + 360;
            continue;
        end
        break;
    end
    
    A1 = b - a;
    A2 = (b + 360) - a;
    A3 = (b - 360) - a;

    A1_abs = abs(A1);
    A2_abs = abs(A2);
    A3_abs = abs(A3);

    if(A1_abs < A2_abs)
        
        if(A1_abs < A3_abs)
            out = A1;
            return;
        else
            out = A3;
            return;
        end

    else
        
        if(A1_abs < A3_abs)
            out = A2;
            return;
        else
            if(A2_abs < A3_abs)
                out = A2;
                return;
            else
                out = A3;
                return;
            end
        end

    end
end