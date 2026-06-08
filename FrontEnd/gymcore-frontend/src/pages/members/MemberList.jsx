import { useEffect, useState } from "react";
import { Link } from "react-router-dom";

import DashboardLayout from "../../layouts/DashboardLayout";

import MemberCard from "../../components/MemberCard";

import {
    getMembers,
    deactivateMember
} from "../../api/memberApi";

function MemberList() {

    const [members, setMembers] =
        useState([]);

    const [searchTerm, setSearchTerm] =
        useState("");

    useEffect(() => {

        loadMembers();

    }, []);

    const loadMembers = async () => {

        try {

            const response =
                await getMembers();

            setMembers(response.data);

        }
        catch (error) {

            console.error(error);
        }
    };

    const handleDeactivate =
        async (id) => {

            if (
                !window.confirm(
                    "Deactivate member?"
                )
            )
                return;

            await deactivateMember(id);

            loadMembers();
        };

    const filteredMembers =
        members.filter(member =>
            member.fullName
                .toLowerCase()
                .includes(
                    searchTerm.toLowerCase()
                )
        );

    return (

        <DashboardLayout>

            <div className="d-flex justify-content-between align-items-center mb-4">

                <div>

                    <h2>Members</h2>

                    <p style={{ color: 'white' }}>
                        Manage gym members
                    </p>

                </div>

                <Link
                    to="/members/create"
                    className="btn btn-success"
                >
                    + New Member
                </Link>

            </div>

            <input
                type="text"
                className="form-control mb-4"
                placeholder="Search members..."
                value={searchTerm}
                onChange={(e) =>
                    setSearchTerm(
                        e.target.value
                    )
                }
            />

            <div className="row">

                {filteredMembers.map(
                    member => (

                        <div
                            key={member.id}
                            className="col-md-4 mb-4"
                        >

                            <MemberCard
                                member={member}
                                onDeactivate={
                                    handleDeactivate
                                }
                            />

                        </div>
                    )
                )}

            </div>

        </DashboardLayout>
    );
}

export default MemberList;