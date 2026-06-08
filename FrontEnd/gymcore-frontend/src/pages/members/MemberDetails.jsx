import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

import DashboardLayout from "../../layouts/DashboardLayout";

import { getMember } from "../../api/memberApi";

function MemberDetails() {

    const { id } = useParams();

    const [member, setMember] =
        useState(null);

    useEffect(() => {

        loadMember();

    }, []);

    const loadMember = async () => {

        const response =
            await getMember(id);

        setMember(response.data);
    };

    if (!member)
        return <p>Loading...</p>;

    return (

        <DashboardLayout>

            <div className="card">

                <div className="card-body">

                    <h2>
                        {member.fullName}
                    </h2>

                    <hr />

                    <p>
                        <strong>Email:</strong>
                        {" "}
                        {member.email}
                    </p>

                    <p>
                        <strong>Phone:</strong>
                        {" "}
                        {member.phoneNumber}
                    </p>

                    <p>
                        <strong>Gender:</strong>
                        {" "}
                        {member.gender}
                    </p>

                    <p>
                        <strong>Plan:</strong>
                        {" "}
                        {member.membershipPlanName}
                    </p>

                </div>

            </div>

        </DashboardLayout>
    );
}

export default MemberDetails;